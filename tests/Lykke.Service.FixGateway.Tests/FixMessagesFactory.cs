using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests
{
    public static class FixMessagesFactory
    {
        public static MarketDataRequest CreateMarketDataRequest(string assetPair = "BTCUSD", string id = "34", bool bid = true, bool ask = true, int depth = 0)
        {
            var request = new MarketDataRequest
            {
                MDReqID = new MDReqID(id),
                SubscriptionRequestType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES),
                MarketDepth = new MarketDepth(depth),
                NoMDEntryTypes = new NoMDEntryTypes(1),
                NoRelatedSym = new NoRelatedSym(1)
            };
            if (ask)
            {
                var typeGroup = new MarketDataRequest.NoMDEntryTypesGroup
                {
                    MDEntryType = new MDEntryType(MDEntryType.OFFER)
                };
                request.AddGroup(typeGroup);
            }

            if (bid)
            {
                var typeGroup = new MarketDataRequest.NoMDEntryTypesGroup
                {
                    MDEntryType = new MDEntryType(MDEntryType.BID)
                };
                request.AddGroup(typeGroup);
            }


            var symbolGroup = new MarketDataRequest.NoRelatedSymGroup
            {
                Symbol = new Symbol(assetPair)
            };

            request.AddGroup(symbolGroup);
            return request;
        }
    }
}
