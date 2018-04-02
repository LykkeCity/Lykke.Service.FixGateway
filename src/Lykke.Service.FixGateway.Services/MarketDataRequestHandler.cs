using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
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
    public sealed class MarketDataRequestHandler : IMarketDataRequestHandler
    {
        private readonly IMarketDataRequestValidator _requestValidator;
        private readonly IObservable<OrderBook> _messageProducer;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ILog _log;
        private readonly ConcurrentDictionary<string, Subscription> _subscriptions = new ConcurrentDictionary<string, Subscription>();
        private readonly CancellationTokenSource _tokenSource;
        private IDisposable _orderBookSubscription;

        public MarketDataRequestHandler(IMarketDataRequestValidator requestValidator, IObservable<OrderBook> messageProducer, IFixMessagesSender fixMessagesSender, ILog log)
        {
            _requestValidator = requestValidator;
            _messageProducer = messageProducer;
            _fixMessagesSender = fixMessagesSender;
            _log = log.CreateComponentScope(nameof(MarketDataRequestHandler));
            _tokenSource = new CancellationTokenSource();
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


        public Task Handle(MarketDataRequest request)
        {
            return HandleRequestAsync(request);
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
                if (!await _requestValidator.ValidateAsync(request, _subscriptions.Keys, _tokenSource.Token))
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
                var erroCode = Guid.NewGuid();
                await _log.WriteWarningAsync(nameof(HandleRequestAsync), $"Unable to handle market data request. MarketDataRequest.MDReqID: {request.MDReqID.Obj}  Error code {erroCode}", "", ex);
                var reject = new Reject().CreateReject(request, erroCode);
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






        private void Send(Message message)
        {
            _fixMessagesSender.Send(message);
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

        public void Init()
        {
            _orderBookSubscription = _messageProducer.Subscribe(OrderBookReceived);
        }
    }
}
