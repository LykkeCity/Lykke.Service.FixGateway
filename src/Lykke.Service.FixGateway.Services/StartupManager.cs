using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Services.DTO.MatchingEngine;

namespace Lykke.Service.FixGateway.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.

    [UsedImplicitly]
    public sealed class StartupManager : IStartupManager
    {
        private readonly IEnumerable<ISessionManager> _sessionManagers;
        private readonly MessagesDispatcher<OrderBook> _orderBooksDispatcher;
        private readonly MessagesDispatcher<MarketOrderWithTrades> _marketOrdersDispatcher;

        public StartupManager(IEnumerable<ISessionManager> sessionManagers, MessagesDispatcher<OrderBook> orderBooksDispatcher, MessagesDispatcher<MarketOrderWithTrades> marketOrdersDispatcher)
        {
            _sessionManagers = sessionManagers;
            _orderBooksDispatcher = orderBooksDispatcher;
            _marketOrdersDispatcher = marketOrdersDispatcher;
        }

        public async Task StartAsync()
        {
            foreach (var manager in _sessionManagers)
            {
                manager.Start();
            }
            _orderBooksDispatcher.Start();
            _marketOrdersDispatcher.Start();

            await Task.CompletedTask;
        }
    }
}
