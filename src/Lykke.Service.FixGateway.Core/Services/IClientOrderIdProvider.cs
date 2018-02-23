using System;
using System.Threading.Tasks;
using Autofac;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IClientOrderIdProvider : IStartable
    {
        Task RegisterNewOrderAsync(Guid orderId, string clientOrderId);
        Task<bool> CheckExistsAsync(string clientOrderId);
        Task RemoveCompletedAsync(Guid orderId);
        Task<(bool hasValue, string clientOrderId)> TryGetClientOrderIdByOrderIdAsync(Guid orderId);
        Task<Guid> GetOrderIdByClientOrderId(string clientOrderId);
    }
}
