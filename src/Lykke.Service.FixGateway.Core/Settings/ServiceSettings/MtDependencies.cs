using JetBrains.Annotations;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class MtDependencies
    {
        public MarginTradingClientSettings MarginTradingClientSettings { get; set; }

    }
}