using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Services.Extensions;
using QuickFix.Fields;
using QuickFix.FIX44;
using ILog = Common.Log.ILog;
using Message = QuickFix.Message;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class SecurityListRequestHandler : ISecurityListRequestHandler
    {
        private readonly IAssetsService _assetsService;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ISecurityListRequestValidator _requestValidator;
        private readonly ILog _log;
        private readonly TimeSpan _defaultRequestTimeout = TimeSpan.FromSeconds(30);
        private readonly CancellationTokenSource _tokenSource;


        public SecurityListRequestHandler(IAssetsService assetsService, IFixMessagesSender fixMessagesSender, ISecurityListRequestValidator requestValidator, ILog log)
        {
            _assetsService = assetsService;
            _fixMessagesSender = fixMessagesSender;
            _requestValidator = requestValidator;
            _log = log.CreateComponentScope(nameof(SecurityListRequestHandler));
            _tokenSource = new CancellationTokenSource();
        }

        public Task Handle(SecurityListRequest request)
        {
            return HandleRequestAsync(request);
        }

        private async Task HandleRequestAsync(SecurityListRequest request)
        {
            try
            {
                if (!_requestValidator.Validate(request))
                {
                    return;
                }
                using (var cts = new CancellationTokenSource(_defaultRequestTimeout))
                using (var cts2 = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _tokenSource.Token))
                {
                    var assetPairs = await _assetsService.GetAllAssetPairsAsync(cts2.Token);
                    var response = GetSuccessfulResponse(request, assetPairs);
                    Send(response);

                }
            }
            catch (Exception ex)
            {
                var errorCode = Guid.NewGuid();
                _log.WriteWarning(nameof(HandleRequestAsync), "", $"SecurityListRequest. Id: {request.SecurityReqID.Obj}. Error code: {errorCode}", ex);
                var reject = new Reject().CreateReject(request, errorCode);
                Send(reject);
            }
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

            foreach (var pair in pairs)
            {
                var gr = new SecurityList.NoRelatedSymGroup
                {
                    Symbol = new Symbol(pair.Id)
                };
                resp.AddGroup(gr);
            }

            return resp;
        }


        private void Send(Message reject)
        {
            _fixMessagesSender.Send(reject);
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}
