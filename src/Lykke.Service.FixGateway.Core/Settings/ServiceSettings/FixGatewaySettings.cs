namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    public sealed class FixGatewaySettings
    {
        public Credentials Credentials { get; set; }
        public DbSettings Db { get; set; }
        public Sessions Sessions { get; set; }
        public RabbitMqLykkeConfiguration RabbitMq { get; set; }
    }
}
