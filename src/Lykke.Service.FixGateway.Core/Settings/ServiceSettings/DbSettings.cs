using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
