using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Services.Extensions;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class SecurityListRequestValidator : ISecurityListRequestValidator
    {
        private readonly IFixMessagesSender _messagesSender;

        public SecurityListRequestValidator(IFixMessagesSender messagesSender)
        {
            _messagesSender = messagesSender;
        }

        public bool Validate(SecurityListRequest request)
        {
            if (request.SecurityListRequestType.Obj != SecurityListRequestType.SYMBOL)
            {
                var reject = new SecurityList().CreateReject(request, new SecurityRequestResult(SecurityRequestResult.INVALID_OR_UNSUPPORTED_REQUEST));
                _messagesSender.Send(reject);
                return false;
            }
            return true;
        }
    }
}

