using Autofac;
using Lykke.Service.FixGateway.Core.Settings;
using Lykke.SettingsReader;
using StackExchange.Redis;

namespace Lykke.Service.FixGateway.Modules
{
    public sealed class RedisModule : Module
    {
        private readonly IReloadingManager<RedisSettings> _settings;

        public RedisModule(IReloadingManager<RedisSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => ConnectionMultiplexer.Connect(_settings.CurrentValue.Configuration))
                .As<IConnectionMultiplexer>();
        }
    }
}
