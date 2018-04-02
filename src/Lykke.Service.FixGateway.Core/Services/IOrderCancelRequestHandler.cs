using System.Threading.Tasks;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IOrderCancelRequestHandler : IRequestHandler<OrderCancelRequest>
    {
        Task Handle(OrderCancelRequest request);
    }
}
