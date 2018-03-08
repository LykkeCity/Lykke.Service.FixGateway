using Autofac;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services.DTO.MatchingEngine;
using Lykke.SettingsReader;
using Microsoft.ApplicationInsights.Extensibility;

namespace Lykke.Service.FixGateway.Modules
{
    public sealed class RabbitMqModule : Module
    {
        private readonly IReloadingManager<FixGatewaySettings> _settings;
        private readonly ILog _log;

        public RabbitMqModule(IReloadingManager<FixGatewaySettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

        }

        protected override void Load(ContainerBuilder builder)
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
