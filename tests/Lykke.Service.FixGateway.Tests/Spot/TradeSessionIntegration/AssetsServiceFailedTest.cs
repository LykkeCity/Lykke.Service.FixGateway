using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Autofac;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;
using NSubstitute;
using NUnit.Framework;

namespace Lykke.Service.FixGateway.Tests.Spot.TradeSessionIntegration
{
    [TestFixture]
    internal class AssetsServiceFailedTest : ExternalServiceFailedBase
    {
        protected override void InitContainer(LocalSettingsReloadingManager<AppSettings> appSettings, ContainerBuilder builder)
        {
            base.InitContainer(appSettings, builder);
            var ocProxy = Substitute.For<IAssetsService>();
            ocProxy.GetAllAssetPairsAsync().ReturnsForAnyArgs(Task.FromException<IReadOnlyCollection<AssetPair>>(new SocketException()));
            ocProxy.TryGetAssetPairAsync("").ReturnsForAnyArgs(Task.FromException<AssetPair>(new SocketException()));

            builder.RegisterInstance(ocProxy)
                .As<IAssetsService>();
        }

    }
}
