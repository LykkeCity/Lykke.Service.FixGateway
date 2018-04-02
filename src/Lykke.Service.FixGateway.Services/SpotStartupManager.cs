using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Services.DTO.MatchingEngine;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class SpotStartupManager : StartupManager
    {
        private readonly MessagesDispatcher<OrderBook> _orderBooksDispatcher;
        private readonly MessagesDispatcher<MarketOrderWithTrades> _marketOrdersDispatcher;
        private readonly MessagesDispatcher<LimitOrdersReport> _limitOrderDispatcher;
        private readonly TcpMatchingEngineClient _matchingEngineClient;

        public SpotStartupManager(IEnumerable<ISessionManager> sessionManagers,
            MessagesDispatcher<MarketOrderWithTrades> marketOrdersDispatcher,
            MessagesDispatcher<OrderBook> orderBooksDispatcher,
            MessagesDispatcher<LimitOrdersReport> limitOrderDispatcher,
            TcpMatchingEngineClient matchingEngineClient) : base(sessionManagers)
        {
            _orderBooksDispatcher = orderBooksDispatcher;
            _marketOrdersDispatcher = marketOrdersDispatcher;
            _limitOrderDispatcher = limitOrderDispatcher;
            _matchingEngineClient = matchingEngineClient;
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();
            _orderBooksDispatcher.Init();
            _marketOrdersDispatcher.Init();
            _limitOrderDispatcher.Init();
            _matchingEngineClient.Start();
        }
    }
}
