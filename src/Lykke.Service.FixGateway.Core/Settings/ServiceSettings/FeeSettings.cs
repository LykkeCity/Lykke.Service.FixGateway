namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    public sealed class FeeSettings
    {
        public TargetClientId TargetClientId { get; set; }
    }

    public sealed class TargetClientId
    {
        public string Hft { get; set; }
    }
}
