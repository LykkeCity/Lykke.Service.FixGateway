using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    public sealed class MarginTradingClientSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }

        public string ApiKey { get; set; }
    }
}