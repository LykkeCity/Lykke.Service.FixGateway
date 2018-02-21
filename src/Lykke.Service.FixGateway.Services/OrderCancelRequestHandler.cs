using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace Lykke.Service.FixGateway.Services
{
    public class OrderCancelRequestHandler: IRequestHandler<OrderCancelRequest>
    {
        private readonly Credentials _credentials;
        private readonly SessionState _sessionState;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ILog _log;
        private readonly CancellationTokenSource _tokenSource;
        private readonly IClientOrderIdProvider _clientOrderIdProvider;




        public void Handle(OrderCancelRequest request)
        {
            Task.Factory.StartNew(HandleRequestAsync, request, _tokenSource.Token).Unwrap().GetAwaiter().GetResult();

        }

        private async Task HandleRequestAsync(object input)
        {
//            var request = (OrderCancelRequest)input;
//            var newOrderId = GenerateOrderId();
//            try
//            {
//                if (!await ValidateRequestAsync(newOrderId, request))
//                {
//                    return;
//                }
//
//                await _clientOrderIdProvider.RegisterNewOrderAsync(newOrderId, request.ClOrdID.Obj);
//
//                if (IsMarketOrder(request))
//                {
//                    await HandleMarketOrderAsync(newOrderId, request);
//                }
//                else
//                {
//                    await HandleLimitOrderAsync(newOrderId, request);
//                }
//            }
//            catch (Exception ex)
//            {
//                await _log.WriteWarningAsync(nameof(HandleRequestAsync), $"NewOrederRequest. Id {newOrderId}", "", ex);
//                var reject = CreateRejectResponse(newOrderId, request, OrdRejReason.OTHER);
//                Send(reject);
//            }
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

        
//        private async Task<bool> ValidateRequestAsync(Guid newOrderId, NewOrderSingle request)
//        {
//            var rejectReason = await _orderCharacteristicsValidator.ValidateRequestAsync(request);
//            if (rejectReason != null)
//            {
//                var reject = CreateRejectResponse(newOrderId, request, rejectReason);
//                Send(reject);
//                return false;
//            }
//            return true;
//        }
    }
}
