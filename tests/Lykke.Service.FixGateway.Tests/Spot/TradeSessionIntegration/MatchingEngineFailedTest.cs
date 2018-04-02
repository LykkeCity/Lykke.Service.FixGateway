using System.Net.Sockets;
using System.Threading.Tasks;
using Autofac;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;
using NSubstitute;
using NUnit.Framework;

namespace Lykke.Service.FixGateway.Tests.Spot.TradeSessionIntegration
{
    [TestFixture, Explicit]
    internal class MatchingEngineFailedTest : ExternalServiceFailedBase
    {
        protected override void InitContainer(LocalSettingsReloadingManager<AppSettings> appSettings, ContainerBuilder builder)
        {
            base.InitContainer(appSettings, builder);
            var ocProxy = Substitute.For<IMatchingEngineClient>();
            ocProxy.HandleMarketOrderAsync(null).ReturnsForAnyArgs(Task.FromException<MarketOrderResponse>(new SocketException()));
            ocProxy.PlaceLimitOrderAsync(null).ReturnsForAnyArgs(Task.FromException<MeResponseModel>(new SocketException()));
            ocProxy.CancelLimitOrderAsync(null).ReturnsForAnyArgs(Task.FromException<MeResponseModel>(new SocketException()));

            builder.RegisterInstance(ocProxy)
                .As<IMatchingEngineClient>();
        }

    }
}
