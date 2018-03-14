using Autofac;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Services;
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
            builder.RegisterType<TcpMatchingEngineClient>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.MatchingEngineClient.IpEndpoint.GetClientIpEndPoint()))
                .WithParameter(TypedParameter.From(false))
                .SingleInstance()
                .As<IMatchingEngineClient>()
                .AsSelf();
        }
    }
}
