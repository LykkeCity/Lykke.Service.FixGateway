using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.Operations.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.FixGateway.Modules
{
    public sealed class ClientModules : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        private readonly IServiceCollection _services;

        public ClientModules(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            _services.RegisterAssetsClient(AssetServiceSettings.Create(new Uri(_settings.CurrentValue.Assets.ServiceUrl), _settings.CurrentValue.Assets.CacheExpirationPeriod));
            builder.RegisterFeeCalculatorClientWithCache(_settings.CurrentValue.FeeCalculatorServiceClient.ServiceUrl, _settings.CurrentValue.FeeCalculatorServiceClient.CacheExpirationPeriod, _log);
            builder.RegisterOperationsClient(_settings.CurrentValue.OperationsServiceClient.ServiceUrl);
            builder.Populate(_services);
            builder.RegisterInstance(_settings.CurrentValue.FeeSettings)
                .SingleInstance();
        }
    }
}
