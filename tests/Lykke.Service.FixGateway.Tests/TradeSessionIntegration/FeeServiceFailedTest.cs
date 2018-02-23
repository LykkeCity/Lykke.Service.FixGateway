using System.Net.Sockets;
using Autofac;
using Lykke.Service.FeeCalculator.AutorestClient.Models;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Lykke.Service.FixGateway.Tests.TradeSessionIntegration
{
    [TestFixture, Explicit]
    internal class FeeServiceFailedTest : ExternalServiceFailedBase
    {
        protected override void InitContainer(LocalSettingsReloadingManager<AppSettings> appSettings, ContainerBuilder builder)
        {
            base.InitContainer(appSettings, builder);
            var ocProxy = Substitute.For<IFeeCalculatorClient>();

            ocProxy.GetMarketOrderAssetFee("", "", "", OrderAction.Buy).ThrowsForAnyArgs(new SocketException());
            ocProxy.GetLimitOrderFees("", "", "", OrderAction.Buy).ThrowsForAnyArgs(new SocketException());

            builder.RegisterInstance(ocProxy)
                .As<IFeeCalculatorClient>();
        }

    }
}
