using Autofac;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;

namespace Lykke.Service.FixGateway.Modules
{
    public sealed class MatchingEngineModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public MatchingEngineModule(
             IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.BindMeClient(_settings.CurrentValue.MatchingEngineClient.IpEndpoint.GetClientIpEndPoint(), socketLog: null, ignoreErrors: true);
        }
    }
}
