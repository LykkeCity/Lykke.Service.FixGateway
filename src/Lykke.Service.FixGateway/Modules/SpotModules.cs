using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.Assets.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services;
using Lykke.Service.FixGateway.Services.Adapters;
using Lykke.Service.FixGateway.Services.DTO.MatchingEngine;
using Lykke.Service.Operations.Client;
using Lykke.SettingsReader;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.FixGateway.Modules
{
    public sealed class SpotModules : Module
    {
        private readonly IReloadingManager<SpotDependencies> _settings;
        private readonly ILog _log;

        private readonly IServiceCollection _services;

        public SpotModules(IReloadingManager<SpotDependencies> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SpotAssetsServiceAdapter>()
                .As<Core.Services.IAssetsService>();

            
            builder.RegisterType<SpotStartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            RegisterClients(builder);
            RegisterRabbitMq(builder);
        }

        private void RegisterClients(ContainerBuilder builder)
        {
            _services.RegisterAssetsClient(AssetServiceSettings.Create(new Uri(_settings.CurrentValue.Assets.ServiceUrl), _settings.CurrentValue.Assets.CacheExpirationPeriod));
            builder.RegisterFeeCalculatorClientWithCache(_settings.CurrentValue.FeeCalculatorServiceClient.ServiceUrl, _settings.CurrentValue.FeeCalculatorServiceClient.CacheExpirationPeriod, _log);
            builder.RegisterOperationsClient(_settings.CurrentValue.OperationsServiceClient.ServiceUrl);
            builder.Populate(_services);
            builder.RegisterInstance(_settings.CurrentValue.FeeSettings)
                .SingleInstance();

            builder.RegisterType<TcpMatchingEngineClient>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.MatchingEngineClient.IpEndpoint.GetClientIpEndPoint()))
                .WithParameter(TypedParameter.From(false))
                .SingleInstance()
                .As<IMatchingEngineClient>()
                .AsSelf();
        }

        private void RegisterRabbitMq(ContainerBuilder builder)
        {
#if DEBUG
            TelemetryConfiguration.Active.DisableTelemetry = true;
#endif

            var rabbitSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _settings.CurrentValue.RabbitMq.IncomingOrderBooks.ConnectionString,
                ExchangeName = _settings.CurrentValue.RabbitMq.IncomingOrderBooks.Exchange,
                QueueName = _settings.CurrentValue.RabbitMq.IncomingOrderBooks.Queue
            };
            var errorStrategy = new DefaultErrorHandlingStrategy(_log, rabbitSettings);
            builder.Register(c => new RabbitMqSubscriber<OrderBook>(rabbitSettings, errorStrategy)
                .SetMessageDeserializer(new JsonMessageDeserializer<OrderBook>())
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                .SetLogger(c.Resolve<ILog>()))
                .SingleInstance();

            var marketOrdConfig = _settings.CurrentValue.RabbitMq.IncomingMarketOrders;
            var orderSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = marketOrdConfig.ConnectionString,
                ExchangeName = marketOrdConfig.Exchange,
                QueueName = marketOrdConfig.Queue
            };

            var ordersErrorStrategy = new DefaultErrorHandlingStrategy(_log, orderSettings);

            builder.Register(c => new RabbitMqSubscriber<MarketOrderWithTrades>(orderSettings, ordersErrorStrategy)
                .SetMessageDeserializer(new JsonMessageDeserializer<MarketOrderWithTrades>())
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                .SetLogger(c.Resolve<ILog>()))
                .SingleInstance();


            var limitOrdConfig = _settings.CurrentValue.RabbitMq.IncomingLimitOrders;
            var limitOrderSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = limitOrdConfig.ConnectionString,
                ExchangeName = limitOrdConfig.Exchange,
                QueueName = limitOrdConfig.Queue,
                IsDurable = true
            };

            var limitOrdersErrorStrategy = new DefaultErrorHandlingStrategy(_log, limitOrderSettings);
            builder.Register(c => new RabbitMqSubscriber<LimitOrdersReport>(limitOrderSettings, limitOrdersErrorStrategy)
                .SetMessageDeserializer(new JsonMessageDeserializer<LimitOrdersReport>())
                .CreateDefaultBinding()
                .SetLogger(c.Resolve<ILog>()))
                .SingleInstance();
        }
    }
}

