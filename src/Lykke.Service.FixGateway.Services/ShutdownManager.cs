using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
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

        public ShutdownManager(IEnumerable<ISessionManager> sessionManagers, RabbitMqSubscriber<OrderBook> orderBookSubscriber)
        {
            _sessionManagers = sessionManagers;
            _orderBookSubscriber = orderBookSubscriber;
        }

        public async Task StopAsync()
        {
            _orderBookSubscriber.Stop();
            foreach (var manager in _sessionManagers)
            {
                manager.Stop();
            }

            await Task.CompletedTask;
        }
    }
}
