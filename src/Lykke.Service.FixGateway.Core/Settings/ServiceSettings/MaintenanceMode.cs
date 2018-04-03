using JetBrains.Annotations;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    [UsedImplicitly]
    public sealed class MaintenanceMode
    {
        public bool Enabled { get; set; }
        public string Reason { get; set; }
    }
}
