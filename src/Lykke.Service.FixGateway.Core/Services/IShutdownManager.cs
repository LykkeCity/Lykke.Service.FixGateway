using System.Threading.Tasks;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}