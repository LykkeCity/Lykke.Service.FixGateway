using System;
using Common.Log;
using QuickFix;
using ILog = Common.Log.ILog;

namespace Lykke.Service.FixGateway.Core.Services
{
    public sealed class FixMessagesSender : IFixMessagesSender
    {
        private readonly ILog _log;

        public FixMessagesSender(ILog log)
        {
            _log = log.CreateComponentScope(nameof(FixMessagesSender));
        }

        public void Send(Message message, SessionID sessionID)
        {
            try
            {
                var result = Session.SendToTarget(message, sessionID);
                if (!result)
                {
                    _log.WriteWarningAsync(nameof(Send), $"SessionID: {sessionID}", "Unable to send a message. The reason unknown").GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _log.WriteWarningAsync(nameof(Send), $"SessionID: {sessionID}", "Unable to send a message", ex).GetAwaiter().GetResult();
            }

        }
    }
}
