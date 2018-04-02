using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Settings.SlackNotifications;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class AppSettings
    {
        public TradingPlatform TradingPlatform { get; set; }

        public FixGatewaySettings FixGatewayService { get; set; }

        [Optional]
        public SpotDependencies SpotDependencies { get; set; }

        [Optional]
        public MtDependencies MtDependencies { get; set; }

        public RedisSettings RedisSettings { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }

    }
}
