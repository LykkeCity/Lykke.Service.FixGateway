using JetBrains.Annotations;

using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class FixGatewaySettings
    {
        public TradingPlatform TradingPlatform { get; set; }

        
        [Optional]
        public MaintenanceMode MaintenanceMode { get; set; } = new MaintenanceMode();

        public Credentials Credentials { get; set; }
        public DbSettings Db { get; set; }
        public Sessions Sessions { get; set; }

        [Optional]
        public SpotDependencies SpotDependencies { get; set; }

        [Optional]
        public MtDependencies MtDependencies { get; set; }
    }
}
