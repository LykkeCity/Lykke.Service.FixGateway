using System.Threading.Tasks;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface INewOrderRequestHandler : IRequestHandler<NewOrderSingle>
    {
        Task Handle(NewOrderSingle request);
    }
}
