using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using JetBrains.Annotations;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Services.Extensions;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class OrderCancelRequestHandler : IOrderCancelRequestHandler
    {
        private readonly SessionState _sessionState;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ILog _log;
        private readonly CancellationTokenSource _tokenSource;
        private readonly IClientOrderIdProvider _clientOrderIdProvider;
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IOrderCancelRequestValidator _requestValidator;
        private readonly IMapper _mapper;
        private readonly TimeSpan _meRequestTimeout = TimeSpan.FromSeconds(5);


        public OrderCancelRequestHandler(SessionState sessionState, IFixMessagesSender fixMessagesSender, ILog log, IClientOrderIdProvider clientOrderIdProvider, IMatchingEngineClient matchingEngineClient, IOrderCancelRequestValidator requestValidator, IMapper mapper)
        {
            _sessionState = sessionState;
            _fixMessagesSender = fixMessagesSender;
            _log = log;
            _clientOrderIdProvider = clientOrderIdProvider;
            _matchingEngineClient = matchingEngineClient;
            _requestValidator = requestValidator;
            _mapper = mapper;
            _tokenSource = new CancellationTokenSource();
        }


        public Task Handle(OrderCancelRequest request)
        {
            return HandleRequestAsync(request);
        }

        private async Task HandleRequestAsync(OrderCancelRequest request)
        {
            try
            {
                if (!await _requestValidator.ValidateAsync(request))
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
                    case MeStatusCodes.NotFound:
                    case MeStatusCodes.Runtime:
                        response = new OrderCancelReject().CreateReject(request, _mapper.Map<CxlRejReason>(meResponse.Status));
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected ME status {meResponse.Status}");
                }

                Send(response);

            }
            catch (Exception ex)
            {
                var clOrdId = request.ClOrdID.Obj;
                var errorCode = Guid.NewGuid();
                _log.WriteWarning(nameof(HandleRequestAsync), "", $"OrderCancelRequest.ClOrdID: {clOrdId}. Error code: {errorCode}", ex);
                var reject = new Reject().CreateReject(request, errorCode);
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
            _fixMessagesSender.Send(message);
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


    }
}
