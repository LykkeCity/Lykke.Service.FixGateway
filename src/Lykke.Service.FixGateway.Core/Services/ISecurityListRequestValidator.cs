using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface ISecurityListRequestValidator
    {
        bool Validate(SecurityListRequest request);
    }
}
