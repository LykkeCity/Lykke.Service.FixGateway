using System.Net.Sockets;
using Autofac;
using Lykke.Service.Assets.Client;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Lykke.Service.FixGateway.Tests.TradeSessionIntegration
{
    [TestFixture]
    internal class AssetsServiceFailedTest : ExternalServiceFailedBase
    {
        protected override void InitContainer(LocalSettingsReloadingManager<AppSettings> appSettings, ContainerBuilder builder)
        {
            base.InitContainer(appSettings, builder);
            var ocProxy = Substitute.For<IAssetsServiceWithCache>();
            ocProxy.GetAllAssetPairsAsync().ThrowsForAnyArgs<SocketException>();
            ocProxy.TryGetAssetPairAsync("").ThrowsForAnyArgs<SocketException>();

            builder.RegisterInstance(ocProxy)
                .As<IAssetsServiceWithCache>();
        }

    }
}
