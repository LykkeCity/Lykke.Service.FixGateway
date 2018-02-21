using Autofac;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services.DTO.MatchingEngine;
using Lykke.SettingsReader;

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
                .SetLogger(c.Resolve<ILog>()));

            var mos = _settings.CurrentValue.RabbitMq.IncomingMarketOrders;
            var orderSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = mos.ConnectionString,
                ExchangeName = mos.Exchange,
                QueueName = mos.Queue
            };

            var ordersErrorStrategy = new DefaultErrorHandlingStrategy(_log, orderSettings);

            builder.Register(c => new RabbitMqSubscriber<MarketOrderWithTrades>(orderSettings, ordersErrorStrategy)
                .SetMessageDeserializer(new JsonMessageDeserializer<MarketOrderWithTrades>())
                .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                .SetLogger(c.Resolve<ILog>()));
        }
    }
}
