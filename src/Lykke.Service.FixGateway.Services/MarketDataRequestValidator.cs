using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Services.Extensions;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class MarketDataRequestValidator : IMarketDataRequestValidator
    {
        private readonly IAssetsService _assetsService;
        private readonly IFixMessagesSender _messagesSender;

        public MarketDataRequestValidator(IAssetsService assetsService, IFixMessagesSender messagesSender)
        {
            _assetsService = assetsService;
            _messagesSender = messagesSender;
        }

        public async Task<bool> ValidateAsync(MarketDataRequest request, IEnumerable<string> subscriptions, CancellationToken token)
        {
            char? reject = null;
            if (subscriptions.Contains(request.MDReqID.Obj))
            {
                reject = MDReqRejReason.DUPLICATE_MDREQID;
            }
            else if (request.SubscriptionRequestType.Obj != SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES
               && request.SubscriptionRequestType.Obj != SubscriptionRequestType.DISABLE_PREVIOUS_SNAPSHOT_PLUS_UPDATE_REQUEST)
            {
                reject = MDReqRejReason.UNSUPPORTED_SUBSCRIPTIONREQUESTTYPE;
            }
            else if (request.MarketDepth.Obj < 0)
            {
                reject = MDReqRejReason.UNSUPPORTED_MARKETDEPTH;
            }

            var entryInvalid = false;
            for (var i = 1; i <= request.NoMDEntryTypes.Obj; i++)
            {
                var entityType = ((MarketDataRequest.NoMDEntryTypesGroup)request.GetGroup(i, new MarketDataRequest.NoMDEntryTypesGroup())).MDEntryType;

                if (entityType.Obj != MDEntryType.OFFER && entityType.Obj != MDEntryType.BID)
                {
                    entryInvalid = true;
                    break;
                }
            }

            if (entryInvalid)
            {
                reject = MDReqRejReason.UNSUPPORTED_MDENTRYTYPE;
            }

            var allSymbols = (await _assetsService.GetAllAssetPairsAsync(token))
                .Select(ap => ap.Id)
                .ToHashSet();

            for (var i = 1; i <= request.NoRelatedSym.Obj; i++)
            {
                var symbol = ((MarketDataRequest.NoRelatedSymGroup)request.GetGroup(i, new MarketDataRequest.NoRelatedSymGroup())).Symbol.Obj;

                if (!allSymbols.Contains(symbol))
                {
                    reject = MDReqRejReason.UNKNOWN_SYMBOL;
                    break;
                }
            }

            if (reject != null)
            {
                var msg = new MarketDataRequestReject().CreateReject(request, new MDReqRejReason(reject.Value));
                _messagesSender.Send(msg);
                return false;
            }
            return true;
        }
    }
}
