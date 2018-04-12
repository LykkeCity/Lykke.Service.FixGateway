using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Settings.SlackNotifications;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class AppSettings
    {

        public FixGatewaySettings FixGatewayService { get; set; }



        public RedisSettings RedisSettings { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }

    }
}
