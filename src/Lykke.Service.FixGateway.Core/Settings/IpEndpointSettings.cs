using System.Net;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.FixGateway.Core.Settings
{
    public sealed class IpEndpointSettings
    {
        public string InternalHost { get; set; }

        [TcpCheck(nameof(Port))]
        public string Host { get; set; }
        public int Port { get; set; }

        public IPEndPoint GetClientIpEndPoint(bool useInternal = false)
        {
            string host = useInternal ? InternalHost : Host;

            if (IPAddress.TryParse(host, out var ipAddress))
                return new IPEndPoint(ipAddress, Port);

            var addresses = Dns.GetHostAddresses(host);
            return new IPEndPoint(addresses[0], Port);
        }
    }
}
