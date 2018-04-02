using System.Threading.Tasks;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IOrderCancelRequestValidator
    {
        Task<bool> ValidateAsync(OrderCancelRequest request);
    }
}
