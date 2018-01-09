namespace Lykke.Service.FixGateway.Services.DTO.MatchingEngine
{
    public sealed class LimitOrderWithTrades
    {
        public NewLimitOrder Order { get; set; }
        public LimitTradeInfo[] Trades { get; set; }
    }
}
