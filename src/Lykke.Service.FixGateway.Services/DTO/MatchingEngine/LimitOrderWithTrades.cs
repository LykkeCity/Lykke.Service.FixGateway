using System.Collections.Generic;

namespace Lykke.Service.FixGateway.Services.DTO.MatchingEngine
{
    public sealed class LimitOrderWithTrades
    {
        public NewLimitOrder Order { get; set; }
        public IReadOnlyCollection<LimitTradeInfo> Trades { get; set; }
    }
}
