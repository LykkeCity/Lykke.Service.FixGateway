using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using ILog = Common.Log.ILog;
using Message = QuickFix.Message;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class MaintenanceModeManager : IMaintenanceModeManager
    {
        private readonly MaintenanceMode _maintenance;
        private readonly IFixMessagesSender _fixMessagesSender;
        private readonly ILog _log;

        public MaintenanceModeManager(MaintenanceMode maintenance, IFixMessagesSender fixMessagesSender, ILog log)
        {
            _maintenance = maintenance;
            _fixMessagesSender = fixMessagesSender;
            _log = log;
        }

        public bool AllowProcessMessages(Message message, SessionID sessionID)
        {
            if (_maintenance.Enabled)
            {
                _log.WriteInfo(nameof(AllowProcessMessages), "", $"Maintenance mode is active. Ignore {message.GetType().Name} request from  {sessionID}");
                var reject = new BusinessMessageReject
                {
                    RefSeqNum = new RefSeqNum(message.Header.GetInt(Tags.MsgSeqNum)),
                    RefMsgType = new RefMsgType(message.Header.GetString(Tags.MsgType)),
                    BusinessRejectReason = new BusinessRejectReason(BusinessRejectReason.APPLICATION_NOT_AVAILABLE),
                    Text = new Text(_maintenance.Reason)
                };
                _fixMessagesSender.Send(reject, sessionID);
                return false;
            }

            return true;
        }
    }
}
