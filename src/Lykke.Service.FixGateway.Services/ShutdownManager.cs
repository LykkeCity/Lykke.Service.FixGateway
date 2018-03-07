using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;

namespace Lykke.Service.FixGateway.Services
{
    // NOTE: Sometimes, shutdown process should be expressed explicitly. 
    // If this is your case, use this class to manage shutdown.
    // For example, sometimes some state should be saved only after all incoming message processing and 
    // all periodical handler was stopped, and so on.
    [UsedImplicitly]
    public sealed class ShutdownManager : IShutdownManager
    {
        private readonly IEnumerable<ISessionManager> _sessionManagers;
        private readonly RabbitMqSubscriber<OrderBook> _orderBookSubscriber;
        private readonly RabbitMqSubscriber<OrderBook> _marketOrderSubscriber;
        private readonly RabbitMqSubscriber<OrderBook> _limitOrderSubscriber;
        private readonly IFixLogEntityRepository _fixLogEntityRepository;

        public ShutdownManager(IEnumerable<ISessionManager> sessionManagers,
            RabbitMqSubscriber<OrderBook> orderBookSubscriber,
            RabbitMqSubscriber<OrderBook> marketOrderSubscriber,
            RabbitMqSubscriber<OrderBook> limitOrderSubscriber,
            IFixLogEntityRepository fixLogEntityRepository)
        {
            _sessionManagers = sessionManagers;
            _orderBookSubscriber = orderBookSubscriber;
            _marketOrderSubscriber = marketOrderSubscriber;
            _limitOrderSubscriber = limitOrderSubscriber;
            _fixLogEntityRepository = fixLogEntityRepository;
        }

        public async Task StopAsync()
        {
            _orderBookSubscriber.Stop();
            _marketOrderSubscriber.Stop();
            _limitOrderSubscriber.Stop();
            foreach (var manager in _sessionManagers)
            {
                manager.Stop();
            }
            _fixLogEntityRepository.Stop();
            await Task.CompletedTask;
        }
    }
}
