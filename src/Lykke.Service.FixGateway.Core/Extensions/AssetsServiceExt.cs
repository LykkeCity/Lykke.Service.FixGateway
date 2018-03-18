using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;

namespace Lykke.Service.FixGateway.Core.Extensions
{
    public static class AssetsServiceExt
    {
        public static async Task<IReadOnlyCollection<AssetPair>> GetAllEnabledAssetPairsAsync([NotNull] this IAssetsServiceWithCache service, CancellationToken cancellationToken = default)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return (await service.GetAllAssetPairsAsync(cancellationToken)).Where(a => !a.IsDisabled).ToArray();
        }
    }
}
