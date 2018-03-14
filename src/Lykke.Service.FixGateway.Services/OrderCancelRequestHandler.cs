using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.FixGateway.Core.Services;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class OrderCancelRequestHandler : IRequestHandler<OrderCancelRequest>
    {
        private readonly SessionState _sessionState;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ILog _log;
        private readonly CancellationTokenSource _tokenSource;
        private readonly IClientOrderIdProvider _clientOrderIdProvider;
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly TimeSpan _meRequestTimeout = TimeSpan.FromSeconds(5);


        public OrderCancelRequestHandler(SessionState sessionState, IFixMessagesSender fixMessagesSender, ILog log, IClientOrderIdProvider clientOrderIdProvider, IMatchingEngineClient matchingEngineClient)
        {
            _sessionState = sessionState;
            _fixMessagesSender = fixMessagesSender;
            _log = log;
            _clientOrderIdProvider = clientOrderIdProvider;
            _matchingEngineClient = matchingEngineClient;
            _tokenSource = new CancellationTokenSource();
        }


        public void Handle(OrderCancelRequest request)
        {
            Task.Factory.StartNew(HandleRequestAsync, request, _tokenSource.Token).Unwrap().GetAwaiter().GetResult();
        }

        private async Task HandleRequestAsync(object input)
        {
            var request = (OrderCancelRequest)input;

            try
            {
                if (!await ValidateRequestAsync(request))
                {
                    return;
                }
                var orderId = await _clientOrderIdProvider.GetOrderIdByClientOrderId(request.OrigClOrdID.Obj);
                MeResponseModel meResponse;
                using (var timeout = new CancellationTokenSource(_meRequestTimeout))
                using (var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_tokenSource.Token, timeout.Token))
                {
                    meResponse = await _matchingEngineClient.CancelLimitOrderAsync(orderId.ToString(), linkedTokenSource.Token);
                    CheckResponseAndThrowIfNullAsync(meResponse);
                }

                Message response;
                switch (meResponse.Status)
                {
                    case MeStatusCodes.Ok:
                        response = CreteAckResponse(request);
                        break;
                    case MeStatusCodes.AlreadyProcessed:
                        response = CreateRejectResponse(request, CxlRejReason.ALREADY_PENDING);
                        break;
                    case MeStatusCodes.NotFound:
                        response = CreateRejectResponse(request, CxlRejReason.UNKNOWN_ORDER);
                        break;
                    case MeStatusCodes.Runtime:
                        response = CreateRejectResponse(request, CxlRejReason.OTHER);
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected ME status {meResponse.Status}");
                }

                Send(response);

            }
            catch (Exception ex)
            {
                var clOrdId = request.ClOrdID.Obj;
                _log.WriteWarning(nameof(HandleRequestAsync), $"OrderCancelRequest. Id {clOrdId}", "", ex);
                var reject = CreateRejectResponse(request, CxlRejReason.OTHER);
                Send(reject);
            }
        }


        private void CheckResponseAndThrowIfNullAsync(object response)
        {
            if (response == null)
            {
                var exception = new InvalidOperationException("ME not available");
                _log.WriteError(nameof(NewOrderRequestHandler), nameof(CheckResponseAndThrowIfNullAsync), exception);
                throw exception;
            }
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }


        private void Send(Message message)
        {
            _fixMessagesSender.Send(message, _sessionState.SessionID);
        }


        private async Task<bool> ValidateRequestAsync(OrderCancelRequest request)
        {
            if (!await _clientOrderIdProvider.CheckExistsAsync(request.OrigClOrdID.Obj))
            {
                var reject = CreateRejectResponse(request, CxlRejReason.UNKNOWN_ORDER);
                Send(reject);
                return false;
            }

            return true;
        }

        private Message CreteAckResponse(OrderCancelRequest request)
        {
            var ack = new ExecutionReport
            {
                OrderID = new OrderID(Guid.NewGuid().ToString()),
                ClOrdID = new ClOrdID(request.ClOrdID.Obj),
                OrigClOrdID = new OrigClOrdID(request.OrigClOrdID.Obj),
                ExecID = new ExecID(_sessionState.NextOrderReportId.ToString()),
                OrdStatus = new OrdStatus(OrdStatus.PENDING_CANCEL),
                ExecType = new ExecType(ExecType.PENDING_CANCEL),
                Symbol = new Symbol("NoSymbol"),
                LeavesQty = new LeavesQty(0),
                CumQty = new CumQty(0),
                AvgPx = new AvgPx(0),
                Side = new Side(Side.BUY)
            };

            return ack;
        }

        private Message CreateRejectResponse(OrderCancelRequest request, int rejectReason, string rejectDescription = null)
        {
            var ordId = rejectReason == CxlRejReason.UNKNOWN_ORDER ? "NONE" : Guid.NewGuid().ToString();
            var reject = new OrderCancelReject
            {
                OrderID = new OrderID(ordId),
                OrigClOrdID = new OrigClOrdID(request.OrigClOrdID.Obj),
                ClOrdID = new ClOrdID(request.ClOrdID.Obj),
                OrdStatus = new OrdStatus(OrdStatus.REJECTED),
                CxlRejReason = new CxlRejReason(rejectReason),
                CxlRejResponseTo = new CxlRejResponseTo(CxlRejResponseTo.ORDER_CANCEL_REQUEST)

            };
            if (rejectDescription != null)
            {
                reject.Text = new Text(rejectDescription);
            }
            return reject;
        }
    }
}
