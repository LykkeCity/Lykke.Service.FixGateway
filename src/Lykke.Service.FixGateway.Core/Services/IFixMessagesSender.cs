using QuickFix;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IFixMessagesSender
    {
        void Send(Message message, SessionID sessionID);
    }
}
