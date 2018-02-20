using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public sealed class LimitOrderNotificationsListener : IDisposable
    {
        private readonly IClientOrderIdProvider _clientOrderIdProvider;
        private readonly SessionState _sessionState;
        private readonly IMapper _mapper;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ILog _log;
        private readonly string _clientId;
        private readonly IDisposable _ordersSubscription;

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
            _sessionState = sessionState;
            _mapper = mapper;
            _fixMessagesSender = fixMessagesSender;
            _log = log.CreateComponentScope(nameof(LimitOrderNotificationsListener));
            _clientId = credentials.ClientId.ToString("D");
            _ordersSubscription = marketOrderSubscriber.Subscribe(async trades => await HandleOrderNotification(trades));
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

            string clientOrderId;
            var orderId = Guid.Parse(order.ExternalId);
            try
            {
                clientOrderId = await _clientOrderIdProvider.GetClientOrderIdByOrderIdAsync(orderId); // ExternalId - the Id we generated in NewOrderRequestHandler
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(HandleOrderNotification), $"Can't find the client order id by the ME order id. ME ExternalID: {order.ExternalId}", ex);
                return;
            }

            var responses = new List<Message>();

            if (trades.Count > 1)
            {
                foreach (var trade in trades.OrderBy(t => t.Timestamp).Take(trades.Count - 1))
                {
                    var partMsg = GetFilledResponse(order, OrdStatus.PARTIALLY_FILLED, clientOrderId, trade);
                    responses.Add(partMsg);
                }
            }

            if (order.Status == OrderStatus.Matched)
            {
                var trade = trades.OrderByDescending(t => t.Timestamp).First();
                var partMsg = GetFilledResponse(order, OrdStatus.FILLED, clientOrderId, trade);
                responses.Add(partMsg);
            }
            else if (order.Status == OrderStatus.Cancelled)
            {
                var partMsg = GetCancelResponse(order, clientOrderId);
                responses.Add(partMsg);
            }
            else if (order.Status == OrderStatus.Processing)
            {
                var trade = trades.OrderByDescending(t => t.Timestamp).First();
                var partMsg = GetFilledResponse(order, OrdStatus.PARTIALLY_FILLED, clientOrderId, trade);
                responses.Add(partMsg);
            }
            else if (order.Status == OrderStatus.InOrderBook)
            {
                // Ignore. We have already sent a response from the API
            }
            else
            {
                responses.Add(CreateFailedResponse(order, clientOrderId));
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
            _fixMessagesSender.Send(message, _sessionState.SessionID);
        }

        public void Dispose()
        {
            _ordersSubscription.Dispose();
        }
    }
}
