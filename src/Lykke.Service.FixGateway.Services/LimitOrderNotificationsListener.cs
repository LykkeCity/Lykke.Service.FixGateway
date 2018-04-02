using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services.DTO.MatchingEngine;
using QuickFix.Fields;
using QuickFix.FIX44;
using ILog = Common.Log.ILog;
using Message = QuickFix.Message;
using OrderStatus = Lykke.Service.FixGateway.Services.DTO.MatchingEngine.OrderStatus;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class LimitOrderNotificationsListener : IDisposable, ISupportInit
    {
        private readonly IClientOrderIdProvider _clientOrderIdProvider;
        private readonly IObservable<LimitOrdersReport> _marketOrderSubscriber;
        private readonly SessionState _sessionState;
        private readonly IMapper _mapper;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ILog _log;
        private readonly string _clientId;
        private IDisposable _ordersSubscription;

        public LimitOrderNotificationsListener(
            Credentials credentials,
            IClientOrderIdProvider clientOrderIdProvider,
            IObservable<LimitOrdersReport> marketOrderSubscriber,
            SessionState sessionState,
            IMapper mapper,
            IFixMessagesSender fixMessagesSender,
            ILog log)
        {
            _clientOrderIdProvider = clientOrderIdProvider;
            _marketOrderSubscriber = marketOrderSubscriber;
            _sessionState = sessionState;
            _mapper = mapper;
            _fixMessagesSender = fixMessagesSender;
            _log = log.CreateComponentScope(nameof(LimitOrderNotificationsListener));
            _clientId = credentials.ClientId.ToString();
        }

        private async Task HandleOrderNotification(LimitOrdersReport ordersReport)
        {
            foreach (var orderWithTrade in ordersReport.Orders)
            {
                await HandleSingleOrder(orderWithTrade);
            }
        }

        private async Task HandleSingleOrder(LimitOrderWithTrades orderWithTrades)
        {

            var order = orderWithTrades.Order;
            var trades = orderWithTrades.Trades;

            if (order.ClientId != _clientId)
            {
                return;
            }

            var orderId = Guid.Parse(order.ExternalId);
            var cachedClientOrderId = await _clientOrderIdProvider.FindClientOrderIdByOrderIdAsync(orderId);
            if (string.IsNullOrEmpty(cachedClientOrderId))
            {
                // Probably the client created|deleted the order via GUI or HFT. The clientOrderId is required field so we can't send an response for this 
                _log.WriteInfo(nameof(HandleSingleOrder), orderId, $"Can't find client order id for {orderId}. It means the client has a parallel session opened via GUI or HFT");
                return;
            }

            var responses = new List<Message>();

            if (trades.Count > 1)
            {
                foreach (var trade in trades.OrderBy(t => t.Timestamp).Take(trades.Count - 1))
                {
                    var partMsg = GetFilledResponse(order, OrdStatus.PARTIALLY_FILLED, cachedClientOrderId, trade);
                    responses.Add(partMsg);
                }
            }

            switch (order.Status)
            {
                case OrderStatus.Matched:
                    {
                        var trade = trades.OrderByDescending(t => t.Timestamp).First();
                        var partMsg = GetFilledResponse(order, OrdStatus.FILLED, cachedClientOrderId, trade);
                        responses.Add(partMsg);
                        break;
                    }
                case OrderStatus.Cancelled:
                    {
                        var partMsg = GetCancelResponse(order, cachedClientOrderId);
                        responses.Add(partMsg);
                        break;
                    }
                case OrderStatus.Processing:
                    {
                        var trade = trades.OrderByDescending(t => t.Timestamp).First();
                        var partMsg = GetFilledResponse(order, OrdStatus.PARTIALLY_FILLED, cachedClientOrderId, trade);
                        responses.Add(partMsg);
                        break;
                    }
                case OrderStatus.InOrderBook:
                    // Ignore. We have already sent a response from the API
                    break;
                default:
                    responses.Add(CreateFailedResponse(order, cachedClientOrderId));
                    break;
            }

            foreach (var response in responses)
            {
                Send(response);
            }
        }

        private Message GetFilledResponse(NewLimitOrder order, char ordStatus, string clientOrderId, LimitTradeInfo trade)
        {
            var msg = new ExecutionReport
            {
                OrderID = new OrderID(order.ExternalId),
                ClOrdID = new ClOrdID(clientOrderId),
                ExecID = new ExecID(_sessionState.NextOrderReportId.ToString()),
                OrdStatus = new OrdStatus(ordStatus),
                ExecType = new ExecType(ExecType.TRADE),
                Symbol = new Symbol(order.AssetPairId),
                OrderQty = new OrderQty(Convert.ToDecimal(Math.Abs(order.Volume))),
                OrdType = new OrdType(OrdType.LIMIT),
                TimeInForce = new TimeInForce(TimeInForce.GOOD_TILL_CANCEL),
                LeavesQty = new LeavesQty(Convert.ToDecimal(Math.Abs(order.RemainingVolume))),
                CumQty = new CumQty(Math.Abs(Convert.ToDecimal(order.Volume - order.RemainingVolume))),
                AvgPx = new AvgPx(0), // Not supported by ME
                LastPx = new LastPx(Convert.ToDecimal(trade.Price)),
                LastQty = new LastQty(Convert.ToDecimal(trade.Volume)),
                TransactTime = new TransactTime(trade.Timestamp),
                Side = order.Volume > 0 ? new Side(Side.BUY) : new Side(Side.SELL)

            };
            return msg;
        }

        private Message GetCancelResponse(NewLimitOrder order, string clientOrderId)
        {
            var msg = new ExecutionReport
            {
                OrderID = new OrderID(order.ExternalId),
                ClOrdID = new ClOrdID(clientOrderId),
                ExecID = new ExecID(_sessionState.NextOrderReportId.ToString()),
                OrdStatus = new OrdStatus(OrdStatus.CANCELED),
                ExecType = new ExecType(ExecType.CANCELED),
                Symbol = new Symbol(order.AssetPairId),
                OrderQty = new OrderQty(Convert.ToDecimal(Math.Abs(order.Volume))),
                OrdType = new OrdType(OrdType.LIMIT),
                TimeInForce = new TimeInForce(TimeInForce.GOOD_TILL_CANCEL),
                LeavesQty = new LeavesQty(0),
                CumQty = new CumQty(Math.Abs(Convert.ToDecimal(order.Volume - order.RemainingVolume))),
                AvgPx = new AvgPx(0), // Not supported by ME
                LastPx = new LastPx(0),
                LastQty = new LastQty(0),
                TransactTime = new TransactTime(DateTime.UtcNow),
                Side = order.Volume > 0 ? new Side(Side.BUY) : new Side(Side.SELL)

            };
            return msg;
        }

        private Message CreateFailedResponse(NewLimitOrder order, string clientOrderId)
        {
            var msg = new ExecutionReport
            {
                OrderID = new OrderID(order.ExternalId),
                ClOrdID = new ClOrdID(clientOrderId),
                ExecID = new ExecID(_sessionState.NextOrderReportId.ToString()),
                OrdStatus = new OrdStatus(OrdStatus.REJECTED),
                ExecType = new ExecType(ExecType.REJECTED),
                Symbol = new Symbol(order.AssetPairId),
                OrderQty = new OrderQty(Convert.ToDecimal(Math.Abs(order.Volume))),
                OrdType = new OrdType(OrdType.LIMIT),
                TimeInForce = new TimeInForce(TimeInForce.GOOD_TILL_CANCEL),
                LeavesQty = new LeavesQty(0),
                CumQty = new CumQty(Math.Abs(Convert.ToDecimal(order.Volume - order.RemainingVolume))),
                AvgPx = new AvgPx(0),
                OrdRejReason = _mapper.Map<OrderStatus, OrdRejReason>(order.Status),
                Text = new Text(order.Status.ToString()),
                Side = order.Volume > 0 ? new Side(Side.BUY) : new Side(Side.SELL)
            };
            return msg;
        }




        private void Send(Message message)
        {
            _fixMessagesSender.Send(message);
        }

        public void Dispose()
        {
            _ordersSubscription?.Dispose();
        }

        public void Init()
        {
            _ordersSubscription = _marketOrderSubscriber.Subscribe(async trades => await HandleOrderNotification(trades));
        }
    }
}
