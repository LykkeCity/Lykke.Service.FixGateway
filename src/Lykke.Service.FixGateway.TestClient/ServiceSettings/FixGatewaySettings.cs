namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    public sealed class FixGatewaySettings
    {
        public string ServiceUrl { get; set; }
        public Credentials Credentials { get; set; }
        public Sessions Sessions { get; set; }
    }
}
