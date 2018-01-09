using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings
{
    public class RabbitMqExchangeConfiguration
    {
        public bool Enabled { get; set; }
        
        [Optional]
        public string Exchange { get; set; }
        [Optional]
        public string Queue { get; set; }
        
        [AmqpCheck]
        public string ConnectionString { get; set; }
    }
}
