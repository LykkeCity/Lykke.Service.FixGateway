using QuickFix;

namespace Lykke.Service.FixGateway.Services
{
    public interface IMaintenanceModeManager
    {
        bool AllowProcessMessages(Message message, SessionID sessionID);
    }
}
