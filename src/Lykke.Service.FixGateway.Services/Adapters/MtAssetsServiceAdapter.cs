using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MarginTrading.Client.AutorestClient;
using Lykke.MarginTrading.Client.AutorestClient.Models;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using IAssetsService = Lykke.Service.FixGateway.Core.Services.IAssetsService;

namespace Lykke.Service.FixGateway.Services.Adapters
{
    [UsedImplicitly]
    public sealed class MtAssetsServiceAdapter : IAssetsService, IDisposable
    {
        private readonly IMarginTradingApi _serviceWithCache;
        private readonly Credentials _credentials;
        private readonly IMapper _mapper;
        private readonly ILog _log;
        private readonly ConcurrentDictionary<string, AssetPair> _cache = new ConcurrentDictionary<string, AssetPair>();
        private readonly Timer _cacheRefreshTimer;
        private volatile bool _stopUpdating;

        public MtAssetsServiceAdapter(IMarginTradingApi marginTradingApi, Credentials credentials, IMapper mapper, ILog log)
        {
            _serviceWithCache = marginTradingApi;
            _credentials = credentials;
            _mapper = mapper;
            _log = log;
            _cacheRefreshTimer = new Timer(ReloadCache);
        }

        public Task<IReadOnlyCollection<AssetPair>> GetAllAssetPairsAsync(CancellationToken cancellationToken = default)
        {
            ReloadForFirstTime();
            return Task.FromResult((IReadOnlyCollection<AssetPair>)_cache.Values);
        }

        public Task<AssetPair> TryGetAssetPairAsync(string id, CancellationToken cancellationToken = default)
        {
            ReloadForFirstTime();
            _cache.TryGetValue(id, out var result);
            return Task.FromResult(result);
        }

        private void ReloadForFirstTime()
        {
            if (_cache.IsEmpty)
            {
                ReloadCache(null);
            }
        }

        private void ReloadCache(object state)
        {
            if (_stopUpdating)
            {
                return;
            }
            try
            {
                var mtPairs = _serviceWithCache.GetAssetsAsync(new ClientIdBackendRequest(_credentials.ClientId.ToString())).GetAwaiter().GetResult();
                var result = _mapper.Map<IReadOnlyCollection<AssetPair>>(mtPairs);
                foreach (var assetPair in result)
                {
                    _cache[assetPair.Id] = assetPair;
                }
            }
            catch (Exception e)
            {
                _log.WriteWarning(nameof(ReloadCache), "", "Unable to load asset lists from MT", e);
            }
            finally
            {
                _cacheRefreshTimer.Change(TimeSpan.FromMinutes(5), Timeout.InfiniteTimeSpan);
            }

        }

        public void Dispose()
        {
            _stopUpdating = true;
            _cacheRefreshTimer.Dispose();
        }
    }
}
