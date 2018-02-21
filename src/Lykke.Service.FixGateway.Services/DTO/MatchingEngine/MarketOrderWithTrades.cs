using System.Collections.Generic;

namespace Lykke.Service.FixGateway.Services.DTO.MatchingEngine
{
    public sealed class MarketOrderWithTrades
    {
        public MarketOrder Order { get; set; }
        public IReadOnlyCollection<TradeInfo> Trades { get; set; }
    }
}
