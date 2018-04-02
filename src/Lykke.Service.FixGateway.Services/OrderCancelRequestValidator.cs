using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Services.Extensions;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class OrderCancelRequestValidator : IOrderCancelRequestValidator
    {
        private readonly IClientOrderIdProvider _clientOrderIdProvider;
        private readonly IFixMessagesSender _fixMessagesSender;

        public OrderCancelRequestValidator(IClientOrderIdProvider clientOrderIdProvider, IFixMessagesSender fixMessagesSender)
        {
            _clientOrderIdProvider = clientOrderIdProvider;
            _fixMessagesSender = fixMessagesSender;
        }

        public async Task<bool> ValidateAsync(OrderCancelRequest request)
        {
            if (!await _clientOrderIdProvider.CheckExistsAsync(request.OrigClOrdID.Obj))
            {
                var reject = new OrderCancelReject().CreateReject(request, new CxlRejReason(CxlRejReason.UNKNOWN_ORDER));
                _fixMessagesSender.Send(reject);
                return false;
            }

            return true;
        }
    }
}
