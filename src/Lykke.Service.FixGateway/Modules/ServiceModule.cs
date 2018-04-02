using System;
using Autofac;
using Common.Log;
using Lykke.Logging;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services;
using Lykke.Service.FixGateway.Services.DTO.MatchingEngine;
using Lykke.SettingsReader;

namespace Lykke.Service.FixGateway.Modules
{
    public sealed class ServiceModule : Module
    {
        private readonly IReloadingManager<FixGatewaySettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies

        public ServiceModule(IReloadingManager<FixGatewaySettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

        }

        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();


            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance();

            builder.RegisterFixLogEntityRepository(_settings.Nested(n => n.Db.FixMessagesLogConnString), "FixGatewayMessagesLog");

            builder.RegisterType<QuoteSessionManager>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Sessions.QuoteSession))
                .As<ISessionManager>()
                .SingleInstance();

            builder.RegisterType<MaintenanceModeManager>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.MaintenanceMode))
                .InstancePerLifetimeScope()
                .As<IMaintenanceModeManager>();


            builder.RegisterInstance(_settings.CurrentValue.Credentials)
                .SingleInstance();



            builder.RegisterType<FixNewOrderRequestValidator>()
                .As<IFixNewOrderRequestValidator>();

            builder.RegisterType<MarketDataRequestValidator>()
                .As<IMarketDataRequestValidator>();

            builder.RegisterType<OrderCancelRequestValidator>()
                .As<IOrderCancelRequestValidator>();

            builder.RegisterType<SecurityListRequestValidator>()
                .As<ISecurityListRequestValidator>();

            builder.RegisterType<MarketOrderNotificationsListener>()
                .InstancePerLifetimeScope();

            builder.RegisterType<LimitOrderNotificationsListener>()
                .InstancePerLifetimeScope();

            builder.RegisterType<OrderCancelRequestHandler>()
                .As<IOrderCancelRequestHandler>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ClientOrderIdProvider>()
                .As<IClientOrderIdProvider>()
                .InstancePerLifetimeScope();

            builder.RegisterType<SecurityListRequestHandler>()
                .As<ISecurityListRequestHandler>()
                .InstancePerLifetimeScope();

            builder.RegisterType<MarketDataRequestHandler>()
                .As<IMarketDataRequestHandler>()
                .InstancePerLifetimeScope();

            builder.RegisterType<FixMessagesSender>()
                .As<IFixMessagesSender>()
                .InstancePerLifetimeScope();

            builder.RegisterType<NewOrderRequestHandler>()
                .As<INewOrderRequestHandler>()
                .InstancePerLifetimeScope();


            builder.RegisterType<TradeSessionManager>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Sessions.TradeSession))
                .WithParameter(TypedParameter.From(_settings.CurrentValue.MaintenanceMode))
                .As<ISessionManager>()
                .AsSelf()
                .SingleInstance();


            builder.RegisterType<MessagesDispatcher<MarketOrderWithTrades>>()
                .As<IObservable<MarketOrderWithTrades>>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MessagesDispatcher<LimitOrdersReport>>()
                .As<IObservable<LimitOrdersReport>>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<MessagesDispatcher<OrderBook>>()
                .As<IObservable<OrderBook>>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
