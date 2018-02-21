using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.FixGateway.Core.Services;
using QuickFix.Fields;
using QuickFix.FIX44;
using ILog = Common.Log.ILog;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class AssetsListRequestHandler : IRequestHandler<SecurityListRequest>
    {
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;
        private readonly SessionState _sessionState;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ILog _log;
        private readonly TimeSpan _defaultRequestTimeout = TimeSpan.FromSeconds(30);
        private readonly CancellationTokenSource _tokenSource;


        public AssetsListRequestHandler(IAssetsServiceWithCache assetsServiceWithCache, SessionState sessionState, IFixMessagesSender fixMessagesSender, ILog log)
        {
            _assetsServiceWithCache = assetsServiceWithCache;
            _sessionState = sessionState;
            _fixMessagesSender = fixMessagesSender;
            _log = log.CreateComponentScope(nameof(AssetsListRequestHandler));
            _tokenSource = new CancellationTokenSource();
        }

        public void Handle(SecurityListRequest request)
        {
            Task.Run(() => HandleRequestAsync(request), _tokenSource.Token); //TODO Review me!
        }

        private async Task HandleRequestAsync(SecurityListRequest request)
        {
            SecurityList response;
            try
            {
                if (!ValidateRequest(request))
                {
                    return;
                }
                using (var cts = new CancellationTokenSource(_defaultRequestTimeout))
                using (var cts2 = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _tokenSource.Token))
                {
                    var assetPairs = await _assetsServiceWithCache.GetAllAssetPairsAsync(cts2.Token);
                    response = GetSuccessfulResponse(request, assetPairs);
                }
            }
            catch (Exception ex)
            {

                await _log.WriteWarningAsync(nameof(HandleRequestAsync), "Unable to receive asset information", "", ex);
                response = GetFailedResponse(request, SecurityRequestResult.INSTRUMENT_DATA_TEMPORARILY_UNAVAILABLE);
            }
            Send(response);
        }

        private static SecurityList GetFailedResponse(SecurityListRequest request, int reason)
        {
            var id = request.SecurityReqID.Obj;
            var resp = new SecurityList
            {
                SecurityReqID = new SecurityReqID(id),
                SecurityResponseID = new SecurityResponseID(id + "-Resp"),
                TotNoRelatedSym = new TotNoRelatedSym(0),
                SecurityRequestResult = new SecurityRequestResult(reason)
            };
            return resp;
        }

        private static SecurityList GetSuccessfulResponse(SecurityListRequest request, IReadOnlyCollection<AssetPair> pairs)
        {
            var id = request.SecurityReqID.Obj;
            var resp = new SecurityList
            {
                SecurityReqID = new SecurityReqID(id),
                SecurityResponseID = new SecurityResponseID(id + "-Resp"),
                TotNoRelatedSym = new TotNoRelatedSym(pairs.Count),
                SecurityRequestResult = new SecurityRequestResult(SecurityRequestResult.VALID_REQUEST)
            };

            foreach (var pair in pairs.Where(p => !p.IsDisabled))
            {
                var gr = new SecurityList.NoRelatedSymGroup
                {
                    Symbol = new Symbol(pair.Id)
                };
                resp.AddGroup(gr);
            }

            return resp;
        }

        private bool ValidateRequest(SecurityListRequest request)
        {
            if (request.SecurityListRequestType.Obj != SecurityListRequestType.SYMBOL)
            {
                var reject = GetFailedResponse(request, SecurityRequestResult.INVALID_OR_UNSUPPORTED_REQUEST);
                Send(reject);
                return false;
            }
            return true;
        }

        private void Send(QuickFix.Message reject)
        {
            _fixMessagesSender.Send(reject, _sessionState.SessionID);
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}
