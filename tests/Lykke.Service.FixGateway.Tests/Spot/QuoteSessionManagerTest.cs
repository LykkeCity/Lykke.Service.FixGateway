using System.Collections.Generic;
using System.Threading;
using Autofac;
using Common.Log;
using Lykke.Logging;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services;
using Lykke.SettingsReader;
using NSubstitute;
using NUnit.Framework;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.FIX44.Message;

namespace Lykke.Service.FixGateway.Tests.Spot
{
    [TestFixture]
    public class QuoteSessionManagerTest
    {
        private QuoteSessionManager _sessionManager;
        private FixClient _fixClient;
        private SessionSetting _sessionSettings;
        private LocalSettingsReloadingManager<AppSettings> _appSettings;
        private IMarketDataRequestHandler _marketDataRequestHandler;
        private IMaintenanceModeManager _maintenanceModeManager;
        private ISecurityListRequestHandler _securityListRequestHandler;
        private SessionID _sessionID;

        [SetUp]
        public void Setup() // СДЕЛАТЬ НОРМАЛЬНЫМ Модульным ТЕСТОМ
        {
            _sessionID = new SessionID("FIX", "A", "B", "");
            _appSettings = new LocalSettingsReloadingManager<AppSettings>("appsettings.Development.json");
            var logRepo = Substitute.For<IFixLogEntityRepository>();

            _sessionSettings = _appSettings.CurrentValue.FixGatewayService.Sessions.QuoteSession;

            _marketDataRequestHandler = Substitute.For<IMarketDataRequestHandler>();
            _maintenanceModeManager = Substitute.For<IMaintenanceModeManager>();
            _securityListRequestHandler = Substitute.For<ISecurityListRequestHandler>();

            _maintenanceModeManager.AllowProcessMessages(Arg.Any<Message>()).Returns(true);


            var builder = new ContainerBuilder();
            builder.RegisterInstance(_marketDataRequestHandler)
                .As<IMarketDataRequestHandler>();
            builder.RegisterInstance(_maintenanceModeManager)
                .As<IMaintenanceModeManager>();
            builder.RegisterInstance(_securityListRequestHandler)
                .As<ISecurityListRequestHandler>();


            _sessionManager = new QuoteSessionManager(_sessionSettings, _appSettings.CurrentValue.FixGatewayService.Credentials, builder.Build(), logRepo, new LogToConsole());
            _fixClient = new FixClient(_sessionSettings.SenderCompID, _sessionSettings.TargetCompID);


            _sessionManager.Init();
            _sessionManager.OnLogon(_sessionID);
        }




        //        [Test]
        //        public void ShouldLogonWithCorrectCredentials()
        //        {
        //            _sessionManager.Init();
        //            _fixClient.Init();
        //
        //            var logon = _fixClient.GetAdminResponse<Message>();
        //            Assert.That(logon, Is.Not.Null);
        //            Assert.That(logon, Is.TypeOf<Logon>());
        //            _fixClient.Stop();
        //        }

        //        [TestCase(true, false)]
        //        [TestCase(false, true)]
        //        [TestCase(true, true)]
        //        public void ShouldNotLogonWithIncorrectCredentials(bool resetTarget, bool resetSender)
        //        {
        //            var client = new FixClient(resetTarget ? "" : _sessionSettings.SenderCompID, resetSender ? "" : _sessionSettings.TargetCompID);
        //            _sessionManager.Init();
        //            client.Init();
        //            var logon = _fixClient.GetAdminResponse<Message>();
        //            Assert.That(logon, Is.Null);
        //            client.Stop();
        //        }


        [Test]
        public void ShouldReturnBusinessReject()
        {

            _maintenanceModeManager.AllowProcessMessages(Arg.Any<SecurityListRequest>()).Returns(false);


            var request = CreateSecurityListRequest();

            _sessionManager.FromApp(request, _sessionID);

            _maintenanceModeManager.Received(1).AllowProcessMessages(Arg.Any<SecurityListRequest>());

        }


        [Test]
        public void ShouldCallMarketDataRequestHandler()
        {

            var request = CreateMarketDataRequest();
            _sessionManager.FromApp(request, _sessionID);

            _marketDataRequestHandler.Received(1).Handle(Arg.Any<MarketDataRequest>());

        }

        [Test]
        public void ShouldCallSecurityListRequestHandler()
        {
            _sessionManager.FromApp(CreateSecurityListRequest(), _sessionID);

            _securityListRequestHandler.Received(1).Handle(Arg.Any<SecurityListRequest>());

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

        
        private static SecurityListRequest CreateSecurityListRequest()
        {
            var request = new SecurityListRequest
            {
                SecurityReqID = new SecurityReqID("42"),
                SecurityListRequestType = new SecurityListRequestType(SecurityListRequestType.ALL_SECURITIES)
            };
            return request;
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
