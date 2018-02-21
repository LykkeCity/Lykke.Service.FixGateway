using System;
using Autofac;
using Common.Log;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Modules;
using Lykke.Service.FixGateway.Services;
using Lykke.SettingsReader;
using NUnit.Framework;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests.TradeSessionIntegration
{
    internal abstract class TradeSessionIntegrationBase : IDisposable
    {
        private TradeSessionManager _sessionManager;
        private IContainer _container;
        protected string ClientOrderId;
        protected FixClient FIXClient;

        [SetUp]
        public virtual void SetUp()
        {
            var appSettings = new LocalSettingsReloadingManager<AppSettings>("appsettings.Development.json");
           var  sessionSetting = appSettings.CurrentValue.FixGatewayService.Sessions.TradeSession;
            var builder = new ContainerBuilder();
            InitContainer(appSettings, builder);
            _container = builder.Build();

            _sessionManager = _container.Resolve<TradeSessionManager>();
            FIXClient = new FixClient(sessionSetting.SenderCompID, sessionSetting.TargetCompID, port: 12357);
            _sessionManager.Start();
            FIXClient.Start();
            ClientOrderId = Guid.NewGuid().ToString("D");
            _container.Resolve<IStartupManager>().StartAsync().GetAwaiter().GetResult();
        }

        [TearDown]
        public virtual void TearDown()
        {
            FIXClient?.Stop();
            _sessionManager?.Stop();
            _sessionManager?.Dispose();
        }

        protected virtual void InitContainer(LocalSettingsReloadingManager<AppSettings> appSettings, ContainerBuilder builder)
        {

            var log = new LogToConsole();

            builder.RegisterInstance(log)
                .As<ILog>();


            builder.RegisterModule(new ServiceModule(appSettings.Nested(x => x.FixGatewayService), log));
            builder.RegisterModule(new ClientModules(appSettings.Nested(x => x), log));
            builder.RegisterModule(new MatchingEngineModule(appSettings.Nested(x => x)));
            builder.RegisterModule(new RedisModule(appSettings.Nested(x => x.RedisSettings)));
            builder.RegisterModule(new AutoMapperModules());
            builder.RegisterModule(new RabbitMqModule(appSettings.Nested(x => x.FixGatewayService), log));
        }





        protected NewOrderSingle CreateNewOrder(bool isMarket = true, bool isBuy = true, string assetPairId = "BTCUSD", decimal qty = 0.1m, decimal? price = null)
        {
            var nos = new NewOrderSingle
            {
                Account = new Account(Const.ClientId),
                ClOrdID = new ClOrdID(ClientOrderId),
                Symbol = new Symbol(assetPairId),
                Side = isBuy ? new Side(Side.BUY) : new Side(Side.SELL),
                OrderQty = new OrderQty(qty),
                OrdType = isMarket ? new OrdType(OrdType.MARKET) : new OrdType(OrdType.LIMIT),
                Price = new Price(price ?? 0M),
                TimeInForce = new TimeInForce(TimeInForce.FILL_OR_KILL),
                TransactTime = new TransactTime(DateTime.UtcNow)
            };
            return nos;
        }

        public void Dispose()
        {
            _container?.Dispose();
        }
    }
}
