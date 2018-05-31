using System;
using System.Threading.Tasks;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IClientOrderIdProvider : ISupportInit
    {
        Task RegisterNewOrderAsync(Guid orderId, string clientOrderId);
        Task<bool> CheckExistsAsync(string clientOrderId);
        Task RemoveCompletedAsync(Guid orderId);
        Task<string> FindClientOrderIdByOrderIdAsync(Guid orderId);
        Task<Guid> GetOrderIdByClientOrderId(string clientOrderId);
    }
}
