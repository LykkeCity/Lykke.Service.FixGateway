using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IMarketDataRequestHandler : IRequestHandler<MarketDataRequest>, ISupportInit
    {
        void Init();
    }
}
