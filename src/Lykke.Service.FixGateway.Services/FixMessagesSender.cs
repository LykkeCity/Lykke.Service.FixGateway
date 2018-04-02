using System;
using Common.Log;
using Lykke.Service.FixGateway.Core.Services;
using QuickFix;
using ILog = Common.Log.ILog;

namespace Lykke.Service.FixGateway.Services
{
    public sealed class FixMessagesSender : IFixMessagesSender
    {
        private readonly SessionState _sessionState;
        private readonly ILog _log;

        public FixMessagesSender(SessionState sessionState, ILog log)
        {
            _sessionState = sessionState;
            _log = log.CreateComponentScope(nameof(FixMessagesSender));
        }

        public void Send(Message message)
        {
            var sessionID = _sessionState.SessionID;
            try
            {
                var result = Session.SendToTarget(message, sessionID);
                if (!result)
                {
                    _log.WriteWarning(nameof(Send), $"SessionID: {sessionID}", "Unable to send a message. The reason unknown");
                }
            }
            catch (Exception ex)
            {
                _log.WriteWarning(nameof(Send), $"SessionID: {sessionID}", "Unable to send a message", ex);
            }

        }
    }
}
