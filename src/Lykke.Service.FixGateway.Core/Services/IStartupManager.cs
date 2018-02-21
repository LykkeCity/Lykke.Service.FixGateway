using System.Threading.Tasks;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}