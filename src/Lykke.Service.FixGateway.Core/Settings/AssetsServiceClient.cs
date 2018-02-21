using System;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings
{
    public sealed class AssetsServiceClient
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
        public TimeSpan CacheExpirationPeriod { get; set; }
    }
}
