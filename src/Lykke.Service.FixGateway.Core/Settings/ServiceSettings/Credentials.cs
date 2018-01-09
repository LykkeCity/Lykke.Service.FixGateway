using System;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    public sealed class Credentials
    {
        public Guid ClientId { get; set; }
        public Guid WalletId { get; set; }
        public string Password { get; set; }
    }
}
