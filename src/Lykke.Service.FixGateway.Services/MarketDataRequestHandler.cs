using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;
using QuickFix.Fields;
using QuickFix.FIX44;
using ILog = Common.Log.ILog;
using Message = QuickFix.Message;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class MarketDataRequestHandler : IRequestHandler<MarketDataRequest>
    {
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;
        private readonly SessionState _sessionState;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ILog _log;
        private readonly ConcurrentDictionary<string, Subscription> _subscriptions = new ConcurrentDictionary<string, Subscription>();
        private readonly CancellationTokenSource _tokenSource;
        private readonly IDisposable _orderBookSubscription;

        public MarketDataRequestHandler(IAssetsServiceWithCache assetsServiceWithCache, IObservable<OrderBook> messageProducer, SessionState sessionState, IFixMessagesSender fixMessagesSender, ILog log)
        {
            _assetsServiceWithCache = assetsServiceWithCache;
            _sessionState = sessionState;
            _fixMessagesSender = fixMessagesSender;
            _log = log.CreateComponentScope(nameof(MarketDataRequestHandler));
            _tokenSource = new CancellationTokenSource();
            _orderBookSubscription = messageProducer.Subscribe(OrderBookReceived);
        }

        private void OrderBookReceived(OrderBook orderBook)
        {
            foreach (var subscription in _subscriptions)
            {
                if (!subscription.Value.AssetPairs.Contains(orderBook.AssetPair))
                {
                    continue;
                }

                var response = new MarketDataSnapshotFullRefresh
                {
                    MDReqID = new MDReqID(subscription.Key),
                    Symbol = new Symbol(orderBook.AssetPair)
                };

                var depth = subscription.Value.Depth == 0 ? int.MaxValue : subscription.Value.Depth;
                var prices = orderBook.IsBuy ? orderBook.Prices.OrderByDescending(p => p.Price) : orderBook.Prices.OrderBy(p => p.Price);
                var entries = new List<MarketDataSnapshotFullRefresh.NoMDEntriesGroup>();
                foreach (var price in prices.Take(depth))
                {
                    if (subscription.Value.Ask && !orderBook.IsBuy)
                    {
                        var ent = new MarketDataSnapshotFullRefresh.NoMDEntriesGroup
                        {
                            MDEntryType = new MDEntryType(MDEntryType.OFFER),
                            MDEntryPx = new MDEntryPx((decimal)price.Price),
                            MDEntrySize = new MDEntrySize((decimal)Math.Abs(price.Volume))
                        };
                        entries.Add(ent);
                    }

                    if (subscription.Value.Bid && orderBook.IsBuy)
                    {
                        var ent = new MarketDataSnapshotFullRefresh.NoMDEntriesGroup
                        {
                            MDEntryType = new MDEntryType(MDEntryType.BID),
                            MDEntryPx = new MDEntryPx((decimal)price.Price),
                            MDEntrySize = new MDEntrySize((decimal)Math.Abs(price.Volume))
                        };
                        entries.Add(ent);
                    }
                }
                response.NoMDEntries = new NoMDEntries(entries.Count);
                foreach (var entry in entries)
                {
                    response.AddGroup(entry);
                }
                Send(response);
            }
        }


        public void Handle(MarketDataRequest request)
        {
            Task.Run(() => HandleRequestAsync(request), _tokenSource.Token);
        }

        private void AbortAllSubscriptions()
        {
            _orderBookSubscription.Dispose();
            _subscriptions.Clear();
            _log.WriteInfo(nameof(AbortAllSubscriptions), "", "Cancel all subscriptions after logout");
        }

        private async Task HandleRequestAsync(MarketDataRequest request)
        {
            try
            {
                if (!await ValidateRequestAsync(request))
                {
                    return;
                }

                if (request.SubscriptionRequestType.Obj == SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES)
                {
                    await SubscribeAsync(request);
                }
                else
                {
                    await UnsubscribeAsync(request);
                }
            }
            catch (Exception ex)
            {
                await _log.WriteWarningAsync(nameof(HandleRequestAsync), "Unable to handle market data request", "", ex);
                var reject = new Reject
                {
                    RefSeqNum = new RefSeqNum(request.Header.GetInt(Tags.MsgSeqNum)),
                    SessionRejectReason = new SessionRejectReason(SessionRejectReason.OTHER)

                };
                Send(reject);
            }
        }

        private async Task UnsubscribeAsync(MarketDataRequest request)
        {
            if (!_subscriptions.TryRemove(request.MDReqID.Obj, out _))
            {
                await _log.WriteWarningAsync("Unsubscribe", "Unsubscribe", $"Unknown subscription Id {request.MDReqID.Obj}");
            }
        }

        private async Task SubscribeAsync(MarketDataRequest request)
        {
            var id = request.MDReqID.Obj;
            var pairs = new HashSet<string>();
            var bid = false;
            var ask = false;

            for (var i = 1; i <= request.NoRelatedSym.Obj; i++)
            {
                var symbol = ((MarketDataRequest.NoRelatedSymGroup)request.GetGroup(i, new MarketDataRequest.NoRelatedSymGroup())).Symbol.Obj;
                pairs.Add(symbol);
            }

            for (var i = 1; i <= request.NoMDEntryTypes.Obj; i++)
            {
                var entityType = ((MarketDataRequest.NoMDEntryTypesGroup)request.GetGroup(i, new MarketDataRequest.NoMDEntryTypesGroup())).MDEntryType;

                switch (entityType.Obj)
                {
                    case MDEntryType.OFFER:
                        ask = true;
                        break;
                    case MDEntryType.BID:
                        bid = true;
                        break;
                }
            }

            _subscriptions.TryAdd(id, new Subscription(pairs, ask, bid, request.MarketDepth.Obj));
            await _log.WriteInfoAsync("Subscribe", "Subscribe", $"Added a new subscription Id {request.MDReqID.Obj}");

        }

        private async Task<bool> ValidateRequestAsync(MarketDataRequest request)
        {
            Message reject = null;
            if (_subscriptions.ContainsKey(request.MDReqID.Obj))
            {
                reject = GetFailedResponse(request, MDReqRejReason.DUPLICATE_MDREQID);
            }
            else if (request.SubscriptionRequestType.Obj != SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES
               && request.SubscriptionRequestType.Obj != SubscriptionRequestType.DISABLE_PREVIOUS_SNAPSHOT_PLUS_UPDATE_REQUEST)
            {
                reject = GetFailedResponse(request, MDReqRejReason.UNSUPPORTED_SUBSCRIPTIONREQUESTTYPE);
            }
            else if (request.MarketDepth.Obj < 0)
            {
                reject = GetFailedResponse(request, MDReqRejReason.UNSUPPORTED_MARKETDEPTH);
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
                reject = GetFailedResponse(request, MDReqRejReason.UNSUPPORTED_MDENTRYTYPE);
            }

            var allSymbols = (await _assetsServiceWithCache.GetAllAssetPairsAsync(_tokenSource.Token)).Where(ap => !ap.IsDisabled)
                .Select(ap => ap.Id)
                .ToHashSet();

            var hasInvalidSymbol = false;
            for (var i = 1; i <= request.NoRelatedSym.Obj; i++)
            {
                var symbol = ((MarketDataRequest.NoRelatedSymGroup)request.GetGroup(i, new MarketDataRequest.NoRelatedSymGroup())).Symbol.Obj;

                if (!allSymbols.Contains(symbol))
                {
                    hasInvalidSymbol = true;
                    break;
                }
            }

            if (hasInvalidSymbol)
            {
                reject = GetFailedResponse(request, MDReqRejReason.UNKNOWN_SYMBOL);
            }

            if (reject != null)
            {
                Send(reject);
                return false;
            }
            return true;
        }

        private static Message GetFailedResponse(MarketDataRequest request, char rejectReason)
        {
            var reject = new MarketDataRequestReject
            {
                MDReqID = request.MDReqID,
                MDReqRejReason = new MDReqRejReason(rejectReason)
            };
            return reject;
        }


        private void Send(Message message)
        {
            _fixMessagesSender.Send(message, _sessionState.SessionID);
        }

        private sealed class Subscription
        {
            public ISet<string> AssetPairs { get; }
            public bool Bid { get; }
            public bool Ask { get; }
            public int Depth { get; }

            public Subscription(ISet<string> assetPairs, bool ask, bool bid, int depth)
            {
                AssetPairs = assetPairs;
                Bid = bid;
                Depth = depth;
                Ask = ask;
            }
        }

        public void Dispose()
        {
            AbortAllSubscriptions();
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}
