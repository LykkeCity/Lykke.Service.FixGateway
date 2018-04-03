using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    public sealed class FixGatewaySettings
    {
        
        [Optional]
        public MaintenanceMode MaintenanceMode { get; set; } = new MaintenanceMode();

        public Credentials Credentials { get; set; }
        public DbSettings Db { get; set; }
        public Sessions Sessions { get; set; }
        public RabbitMqLykkeConfiguration RabbitMq { get; set; }
    }
}
