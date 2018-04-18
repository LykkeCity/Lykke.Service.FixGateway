using System;
using Autofac;
using Common.Log;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Modules;
using Lykke.SettingsReader;
using NUnit.Framework;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests.Spot.TradeSessionIntegration
{
    internal abstract class TradeSessionIntegrationBase : IDisposable
    {
        protected IContainer Container;
        protected string ClientOrderId;
        protected FixClient FIXClient;
        protected LocalSettingsReloadingManager<AppSettings> AppSettings;
        private SessionSetting _sessionSetting;

        [SetUp]
        public virtual void SetUp()
        {
            AppSettings = new LocalSettingsReloadingManager<AppSettings>("appsettings.Development.json");
            _sessionSetting = AppSettings.CurrentValue.FixGatewayService.Sessions.TradeSession;
            var builder = new ContainerBuilder();
            InitContainer(AppSettings, builder);
            Container = builder.Build();
            Container.Resolve<IStartupManager>().StartAsync().GetAwaiter().GetResult();
            FIXClient = new FixClient(_sessionSetting.SenderCompID, _sessionSetting.TargetCompID, port: 12357);
            FIXClient.Init();
            ClientOrderId = Guid.NewGuid().ToString();


        }

        [TearDown]
        public virtual void TearDown()
        {
            FIXClient?.Stop();
            Container?.Resolve<IShutdownManager>().StopAsync().GetAwaiter().GetResult();
            Container?.Dispose();
        }

        protected virtual void InitContainer(LocalSettingsReloadingManager<AppSettings> appSettings, ContainerBuilder builder)
        {

            var log = new LogToConsole();

            builder.RegisterInstance(log)
                .As<ILog>();


            builder.RegisterModule(new ServiceModule(appSettings.Nested(x => x.FixGatewayService), log));
            builder.RegisterModule(new SpotModules(appSettings.Nested(x => x.FixGatewayService.SpotDependencies), log));
            builder.RegisterModule(new RedisModule(appSettings.Nested(x => x.RedisSettings)));
            builder.RegisterModule(new AutoMapperModules());
        }





        public static NewOrderSingle CreateNewOrder(string clientOrderId, bool isMarket = true, bool isBuy = true, string assetPairId = "BTCUSD", decimal qty = 0.1m, decimal? price = null)
        {
            var nos = new NewOrderSingle
            {
                Account = new Account(Const.ClientId),
                ClOrdID = new ClOrdID(clientOrderId),
                Symbol = new Symbol(assetPairId),
                Side = isBuy ? new Side(Side.BUY) : new Side(Side.SELL),
                OrderQty = new OrderQty(qty),
                OrdType = isMarket ? new OrdType(OrdType.MARKET) : new OrdType(OrdType.LIMIT),
                Price = new Price(price ?? 0M),
                TimeInForce = isMarket ? new TimeInForce(TimeInForce.FILL_OR_KILL) : new TimeInForce(TimeInForce.GOOD_TILL_CANCEL),
                TransactTime = new TransactTime(DateTime.UtcNow)
            };
            return nos;
        }

        public void Dispose()
        {
            Container?.Dispose();
        }

    }
}
