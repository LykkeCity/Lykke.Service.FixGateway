using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings
{
    public sealed class OperationsServiceClient
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUri { get; set; }
    }
}
