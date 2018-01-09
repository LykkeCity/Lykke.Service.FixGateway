using System.Threading;
using QuickFix;

namespace Lykke.Service.FixGateway.Services
{
    public sealed class SessionState
    {
        private int _orderReportId;
        public SessionID SessionID { get; }
        public int OrderReportId => _orderReportId;

        public int NextOrderReportId => Interlocked.Increment(ref _orderReportId);

        public SessionState(SessionID sessionID)
        {
            SessionID = sessionID;
        }
    }
}
