using QuickFix;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IMaintenanceModeManager
    {
        bool AllowProcessMessages(Message message);
    }
}
