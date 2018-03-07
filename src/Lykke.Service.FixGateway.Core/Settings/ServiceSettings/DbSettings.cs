using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    [UsedImplicitly]
    public sealed class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        [AzureTableCheck]
        public string FixMessagesLogConnString { get; set; }
    }
}
