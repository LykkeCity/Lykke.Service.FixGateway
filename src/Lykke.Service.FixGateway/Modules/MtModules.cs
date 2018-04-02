using Autofac;
using Lykke.MarginTrading.Client;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services;
using Lykke.SettingsReader;

namespace Lykke.Service.FixGateway.Modules
{
    public sealed class MtModules : Module
    {
        private readonly IReloadingManager<MtDependencies> _settings;


        public MtModules(IReloadingManager<MtDependencies> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MtStartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            RegisterClients(builder);
            RegisterRabbitMq(builder);
        }

        private void RegisterClients(ContainerBuilder builder)
        {
            var set = _settings.CurrentValue.MarginTradingClientSettings;
            builder.RegisterMarginTradingClient(set.ServiceUrl, set.ApiKey);
        }

        private void RegisterRabbitMq(ContainerBuilder builder)
        {
        }
    }
}

