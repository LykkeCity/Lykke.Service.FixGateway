using Lykke.Service.FixGateway.Core.Domain;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IMatchingEngine
    {
        void PlaceMarketOrder(string accountId, AssetPair pair, decimal volume, string linkedOrderId = default);

        void PlaceLimitOrder(string accountId, AssetPair pair, decimal volume, decimal price, string linkedOrderId = default);

        void CancelOrder(string accountId, AssetPair pair);
    }
}
