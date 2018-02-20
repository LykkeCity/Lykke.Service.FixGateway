using System;
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
    public sealed class MarketOrderNotificationsListener : IDisposable
    {
        private readonly IClientOrderIdProvider _clientOrderIdProvider;
        private readonly SessionState _sessionState;
        private readonly IMapper _mapper;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ILog _log;
        private readonly string _clientId;
        private readonly IDisposable _ordersSubscription;

        public MarketOrderNotificationsListener(
            Credentials credentials,
            IClientOrderIdProvider clientOrderIdProvider,
            IObservable<MarketOrderWithTrades> marketOrderSubscriber,
            SessionState sessionState,
            IMapper mapper,
            IFixMessagesSender fixMessagesSender,
            ILog log)
        {
            _clientOrderIdProvider = clientOrderIdProvider;
            _sessionState = sessionState;
            _mapper = mapper;
            _fixMessagesSender = fixMessagesSender;
            _log = log.CreateComponentScope(nameof(MarketOrderNotificationsListener));
            _clientId = credentials.ClientId.ToString("D");
            _ordersSubscription = marketOrderSubscriber.Subscribe(async trades => await HandleMarketOrderNotification(trades));
        }

        private async Task HandleMarketOrderNotification(MarketOrderWithTrades marketOrderWithTrades)
        {
            var marketOrder = marketOrderWithTrades.Order;

            if (marketOrder.ClientId != _clientId)
            {
                return;
            }

            string clientOrderId;
            var orderId = Guid.Parse(marketOrder.ExternalId);
            try
            {
                clientOrderId = await _clientOrderIdProvider.GetClientOrderIdByOrderIdAsync(orderId); // ExternalId - the Id we generated in NewOrderRequestHandler
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(HandleMarketOrderNotification), $"Can't find the client order id by the ME order id. ME ExternalID: {marketOrder.ExternalId}", ex);
                return;
            }
            Message response;

            if (marketOrder.Status == OrderStatus.Matched || marketOrder.Status == OrderStatus.Cancelled)
            {
                response = GetSuccessfulMarketOrderResponse(marketOrder, clientOrderId);
            }
            else
            {
                response = CreateFailedResponse(marketOrder, clientOrderId);
            }
            Send(response);
        }


        private Message GetSuccessfulMarketOrderResponse(MarketOrder marketOrder, string clientOrderId)
        {
            var msg = new ExecutionReport
            {
                OrderID = new OrderID(marketOrder.ExternalId),
                ClOrdID = new ClOrdID(clientOrderId),
                ExecID = new ExecID(_sessionState.NextOrderReportId.ToString()),
                OrdStatus = new OrdStatus(OrdStatus.FILLED),
                ExecType = new ExecType(ExecType.TRADE),
                Symbol = new Symbol(marketOrder.AssetPairId),
                OrderQty = new OrderQty(Convert.ToDecimal(marketOrder.Volume)),
                OrdType = new OrdType(OrdType.MARKET),
                TimeInForce = new TimeInForce(TimeInForce.FILL_OR_KILL),
                LeavesQty = new LeavesQty(0),
                CumQty = new CumQty(Convert.ToDecimal(marketOrder.Volume)),
                AvgPx = new AvgPx(Convert.ToDecimal(marketOrder.Price)),
                LastPx = new LastPx(Convert.ToDecimal(marketOrder.Price)),
                LastQty = new LastQty(Convert.ToDecimal(marketOrder.Volume)),
                TransactTime = new TransactTime(marketOrder.MatchedAt ?? DateTime.UtcNow),
                Side = marketOrder.Volume > 0 ? new Side(Side.BUY) : new Side(Side.SELL)

            };
            return msg;
        }

        private Message CreateFailedResponse(MarketOrder marketOrder, string clientOrderId)
        {
            var msg = new ExecutionReport
            {
                OrderID = new OrderID(marketOrder.ExternalId),
                ClOrdID = new ClOrdID(clientOrderId),
                ExecID = new ExecID(_sessionState.NextOrderReportId.ToString()),
                OrdStatus = new OrdStatus(OrdStatus.REJECTED),
                ExecType = new ExecType(ExecType.REJECTED),
                Symbol = new Symbol(marketOrder.AssetPairId),
                OrderQty = new OrderQty(Convert.ToDecimal(marketOrder.Volume)),
                OrdType = new OrdType(OrdType.MARKET),
                TimeInForce = new TimeInForce(TimeInForce.FILL_OR_KILL),
                LeavesQty = new LeavesQty(0),
                CumQty = new CumQty(0),
                AvgPx = new AvgPx(0),
                OrdRejReason = _mapper.Map<OrderStatus, OrdRejReason>(marketOrder.Status),
                Text = new Text(marketOrder.Status.ToString()),
                Side = marketOrder.Volume > 0 ? new Side(Side.BUY) : new Side(Side.SELL)
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
