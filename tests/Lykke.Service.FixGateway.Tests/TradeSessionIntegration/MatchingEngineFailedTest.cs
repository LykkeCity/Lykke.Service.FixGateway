using System.Net.Sockets;
using Autofac;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Lykke.Service.FixGateway.Tests.TradeSessionIntegration
{
    [TestFixture, Explicit]
    internal class MatchingEngineFailedTest : ExternalServiceFailedBase
    {
        protected override void InitContainer(LocalSettingsReloadingManager<AppSettings> appSettings, ContainerBuilder builder)
        {
            base.InitContainer(appSettings, builder);
            var ocProxy = Substitute.For<IMatchingEngineClient>();
            ocProxy.HandleMarketOrderAsync(null).ThrowsForAnyArgs<SocketException>();
            ocProxy.PlaceLimitOrderAsync(null).ThrowsForAnyArgs<SocketException>();
            ocProxy.CancelLimitOrderAsync(null).ThrowsForAnyArgs<SocketException>();

            builder.RegisterInstance(ocProxy)
                .As<IMatchingEngineClient>();
        }

    }
}
