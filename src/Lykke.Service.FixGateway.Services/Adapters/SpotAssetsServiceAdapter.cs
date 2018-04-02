using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client;
using Lykke.Service.FixGateway.Core.Domain;
using IAssetsService = Lykke.Service.FixGateway.Core.Services.IAssetsService;

namespace Lykke.Service.FixGateway.Services.Adapters
{
    [UsedImplicitly]
    public sealed class SpotAssetsServiceAdapter : IAssetsService
    {
        private readonly IAssetsServiceWithCache _serviceWithCache;
        private readonly IMapper _mapper;

        public SpotAssetsServiceAdapter(IAssetsServiceWithCache serviceWithCache, IMapper mapper)
        {
            _serviceWithCache = serviceWithCache;
            _mapper = mapper;
        }

        public async Task<IReadOnlyCollection<AssetPair>> GetAllAssetPairsAsync(CancellationToken cancellationToken = default)
        {
            var spotAssets = (await _serviceWithCache.GetAllAssetPairsAsync(cancellationToken)).Where(a => !a.IsDisabled).ToArray();
            var result = _mapper.Map<IReadOnlyCollection<AssetPair>>(spotAssets);
            return result;
        }

        public async Task<AssetPair> TryGetAssetPairAsync(string id, CancellationToken cancellationToken = default)
        {
            var spotAss = await _serviceWithCache.TryGetAssetPairAsync(id, cancellationToken);
            var result = _mapper.Map<AssetPair>(spotAss);
            return result;
        }
    }
}
