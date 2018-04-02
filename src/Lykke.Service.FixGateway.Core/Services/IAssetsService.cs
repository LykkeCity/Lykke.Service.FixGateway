using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.FixGateway.Core.Domain;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IAssetsService
    {
        Task<IReadOnlyCollection<AssetPair>> GetAllAssetPairsAsync(CancellationToken cancellationToken = default);
        Task<AssetPair> TryGetAssetPairAsync(string id, CancellationToken cancellationToken = default);
    }
}
