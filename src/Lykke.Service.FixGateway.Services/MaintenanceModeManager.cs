using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using QuickFix.Fields;
using QuickFix.FIX44;
using Message = QuickFix.Message;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class MaintenanceModeManager : IMaintenanceModeManager
    {
        private readonly MaintenanceMode _maintenance;
        private readonly IFixMessagesSender _fixMessagesSender;

        public MaintenanceModeManager(MaintenanceMode maintenance, IFixMessagesSender fixMessagesSender)
        {
            _maintenance = maintenance;
            _fixMessagesSender = fixMessagesSender;
        }

        public bool AllowProcessMessages(Message message)
        {
            if (_maintenance.Enabled)
            {
                var reject = new BusinessMessageReject
                {
                    RefSeqNum = new RefSeqNum(message.Header.GetInt(Tags.MsgSeqNum)),
                    RefMsgType = new RefMsgType(message.Header.GetString(Tags.MsgType)),
                    BusinessRejectReason = new BusinessRejectReason(BusinessRejectReason.APPLICATION_NOT_AVAILABLE),
                    Text = new Text(_maintenance.Reason)
                };
                _fixMessagesSender.Send(reject);
                return false;
            }

            return true;
        }
    }
}
