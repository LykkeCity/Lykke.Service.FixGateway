using System.Collections.Generic;

namespace Lykke.Service.FixGateway.Services.DTO.MatchingEngine
{
    public sealed class LimitOrdersReport
    {
        public IReadOnlyCollection<LimitOrderWithTrades> Orders { get; set; }

    }
}
