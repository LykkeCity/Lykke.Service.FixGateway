using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Modules;
using Lykke.Service.FixGateway.Services;
using Lykke.SettingsReader;
using NSubstitute;
using NUnit.Framework;
using QuickFix.Fields;
using QuickFix.FIX44;
using ILog = Common.Log.ILog;

namespace Lykke.Service.FixGateway.Tests
{
    [TestFixture]
    public class QuoteSessionManagerTest
    {
        private IContainer _container;
        private QuoteSessionManager _sessionManager;
        private IAssetsServiceWithCache _assetsService;
        private FixClient _fixClient;
        private SessionSetting _sessionSettings;
        private Subject<OrderBook> _orderBookSource;

        [SetUp]
        public void Setup()
        {
            var appSettings = new LocalSettingsReloadingManager<AppSettings>("appsettings.Development.json");
            _orderBookSource = new Subject<OrderBook>();

            InitContainer(appSettings);

            _sessionSettings = appSettings.CurrentValue.FixGatewayService.Sessions.QuoteSession;

            _sessionManager = Substitute.ForPartsOf<QuoteSessionManager>(_sessionSettings, appSettings.CurrentValue.FixGatewayService.Credentials,
                _container.Resolve<IAssetsServiceWithCache>(),
                _container.Resolve<ILifetimeScope>(),
                _container.Resolve<ILog>());
            _fixClient = new FixClient(_sessionSettings.SenderCompID, _sessionSettings.TargetCompID);
        }

        private void InitContainer(IReloadingManager<AppSettings> appSettings)
        {

            var log = new LogToConsole();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(log)
                .As<ILog>();


            builder.RegisterModule(new ServiceModule(appSettings.Nested(x => x.FixGatewayService), log));
            builder.RegisterModule(new ClientModules(appSettings.Nested(x => x), log));
            builder.RegisterModule(new MatchingEngineModule(appSettings.Nested(x => x)));
            builder.RegisterModule(new RedisModule(appSettings.Nested(x => x.RedisSettings)));
            builder.RegisterModule(new AutoMapperModules());
            builder.RegisterModule(new RabbitMqModule(appSettings.Nested(x => x.FixGatewayService), log));




            _assetsService = Substitute.For<IAssetsServiceWithCache>();

            builder.RegisterInstance(_assetsService)
                .As<IAssetsServiceWithCache>();

            builder.RegisterInstance(_orderBookSource)
                .As<IObservable<OrderBook>>();


            _container = builder.Build();

        }


        [Test]
        public void ShouldLogonWithCorrectCredentials()
        {
            _sessionManager.Start();
            _fixClient.Start();

            _sessionManager.ReceivedWithAnyArgs(1).OnLogon(null);
            _fixClient.Stop();
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void ShouldNotLogonWithIncorrectCredentials(bool resetTarget, bool resetSender)
        {
            var client = new FixClient(resetTarget ? "" : _sessionSettings.SenderCompID, resetSender ? "" : _sessionSettings.TargetCompID);
            _sessionManager.Start();
            client.Start();

            _sessionManager.DidNotReceiveWithAnyArgs().OnLogon(null);
            client.Stop();
        }

        [Test]
        public void ShouldSendAssetList()
        {
            IReadOnlyCollection<AssetPair> assets = new[]
            {
                new AssetPair
                {
                    Name = "BTC/USD",
                    Id = "BTCUSD"
                }   ,
                new AssetPair
                {
                    Name = "EUR/USD",
                    Id = "EURUSD"
                }
            };
            _assetsService.GetAllAssetPairsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(assets));
            _sessionManager.Start();
            _fixClient.Start();

            var request = new SecurityListRequest
            {
                SecurityReqID = new SecurityReqID("42"),
                SecurityListRequestType = new SecurityListRequestType(SecurityListRequestType.SYMBOL)
            };

            _fixClient.Send(request);
            var resp = _fixClient.GetResponse<SecurityList>();

            Assert.That(resp.SecurityReqID.Obj, Is.EqualTo("42"));
            Assert.That(resp.NoRelatedSym.Obj, Is.EqualTo(2));

            var symGroup = resp.GetGroup(1, new SecurityList.NoRelatedSymGroup());
            Assert.That(symGroup, Is.Not.Null);
            Assert.That(symGroup, Is.TypeOf<SecurityList.NoRelatedSymGroup>());

            var symb = ((SecurityList.NoRelatedSymGroup)symGroup).Symbol;

            Assert.That(symb, Is.Not.Null);
            Assert.That(symb.Obj, Is.EqualTo("BTCUSD"));

        }

        [Test]
        public void ShouldSetCorrectResultIfRequestIncorrect()
        {
            SetupAssetService();
            _sessionManager.Start();
            _fixClient.Start();

            var request = new SecurityListRequest
            {
                SecurityReqID = new SecurityReqID("42"),
                SecurityListRequestType = new SecurityListRequestType(SecurityListRequestType.ALL_SECURITIES)
            };

            _fixClient.Send(request);
            var resp = _fixClient.GetResponse<SecurityList>();

            Assert.That(resp.SecurityReqID.Obj, Is.EqualTo("42"));
            Assert.That(resp.SecurityRequestResult.Obj, Is.EqualTo(SecurityRequestResult.INVALID_OR_UNSUPPORTED_REQUEST));

        }

        [Test]
        public void ShouldSubscribeOnMarketData()
        {
            SetupAssetService();
            _sessionManager.Start();
            _fixClient.Start();


            var request = CreateMarketDataRequest();
            _fixClient.Send(request);
            bool stopPublishing = false;
            Task.Run(() =>
            {
                while (!stopPublishing)
                {
                    Thread.Sleep(1000);
                    _orderBookSource.OnNext(CreateOrderBook());
                    _orderBookSource.OnNext(CreateOrderBook(false));
                }
            });
            Thread.Sleep(10000);
            var resp = _fixClient.GetResponse< QuickFix.Message>();
            stopPublishing = true;
            Assert.That(resp, Is.TypeOf<MarketDataSnapshotFullRefresh>());
        }

        [Test]
        public void ShouldRejectMarketDataSubscription()
        {
            SetupAssetService();
            _sessionManager.Start();
            _fixClient.Start();


            var request = CreateMarketDataRequest("AAAYYYY");
            _fixClient.Send(request);
            bool stopPublishing = false;
            Task.Run(() =>
            {
                while (!stopPublishing)
                {
                    Thread.Sleep(1000);
                    _orderBookSource.OnNext(CreateOrderBook());
                    _orderBookSource.OnNext(CreateOrderBook(false));
                }
            });
            Thread.Sleep(10000);
            var resp = _fixClient.GetResponse<QuickFix.Message>();
            stopPublishing = true;
            Assert.That(resp, Is.TypeOf<MarketDataRequestReject>());
        }

        private static MarketDataRequest CreateMarketDataRequest(string assetPair = "BTCUSD", string id = "34", bool bid = true, bool ask = true)
        {
            var request = new MarketDataRequest
            {
                MDReqID = new MDReqID(id),
                SubscriptionRequestType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES),
                MarketDepth = new MarketDepth(0),
                NoMDEntryTypes = new NoMDEntryTypes(1),
                NoRelatedSym = new NoRelatedSym(1)
            };
            if (ask)
            {
                var typeGroup = new MarketDataRequest.NoMDEntryTypesGroup
                {
                    MDEntryType = new MDEntryType(MDEntryType.OFFER)
                };
                request.AddGroup(typeGroup);
            }

            if (bid)
            {
                var typeGroup = new MarketDataRequest.NoMDEntryTypesGroup
                {
                    MDEntryType = new MDEntryType(MDEntryType.BID)
                };
                request.AddGroup(typeGroup);
            }


            var symbolGroup = new MarketDataRequest.NoRelatedSymGroup
            {
                Symbol = new Symbol(assetPair)
            };

            request.AddGroup(symbolGroup);
            return request;
        }

        private static OrderBook CreateOrderBook(bool isBuy = true, string assetPair = "BTCUSD")
        {
            var oderBook = new OrderBook
            {
                AssetPair = assetPair,
                IsBuy = isBuy,
                Prices = new List<VolumePrice>()
                {
                    new VolumePrice
                    {
                        Price =isBuy?50: 100,
                        Volume = 100000
                    },
                    new VolumePrice
                    {
                        Price =isBuy?150: 200,
                        Volume = 200000
                    },
                }
            };
            return oderBook;
        }

        private void SetupAssetService()
        {
            IReadOnlyCollection<AssetPair> assets = new[]
            {
                new AssetPair
                {
                    Name = "BTC/USD",
                    Id = "BTCUSD"
                }   ,
                new AssetPair
                {
                    Name = "EUR/USD",
                    Id = "EURUSD"
                }
            };
            _assetsService.GetAllAssetPairsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(assets));
        }

        [TearDown]
        public void TearDown()
        {
            _fixClient?.Stop();
            _sessionManager?.Stop();
            _sessionManager?.Dispose();
        }


    }

}
