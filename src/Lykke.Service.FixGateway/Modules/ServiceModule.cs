using System;
using Autofac;
using Common;
using Common.Log;
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

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance();

            builder.RegisterType<QuoteSessionManager>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Sessions.QuoteSession))
                .As<ISessionManager>()
                .SingleInstance();

            builder.RegisterInstance(_settings.CurrentValue.Credentials);

            builder.RegisterType<NewOrderRequestHandler>()
                .InstancePerLifetimeScope();

            builder.RegisterType<MatchingEngineNotificationListener>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ClientOrderIdProvider>()
                .As<IClientOrderIdProvider>()
                .InstancePerLifetimeScope();

            builder.RegisterType<AssetsListRequestHandler>()
                .InstancePerLifetimeScope();

            builder.RegisterType<MarketDataRequestHandler>()
                .InstancePerLifetimeScope();

            builder.RegisterType<TradeSessionManager>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Sessions.TradeSession))
                .As<ISessionManager>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<FixMessagesSender>()
                .As<IFixMessagesSender>()
                .SingleInstance();

            builder.RegisterType<MessagesDispatcher<MarketOrderWithTrades>>()
                .As<IObservable<MarketOrderWithTrades>>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<MessagesDispatcher<OrderBook>>()
                .As<IObservable<OrderBook>>()
                .AsSelf()
                .SingleInstance();
        }

    }
}
