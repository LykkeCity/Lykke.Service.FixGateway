using System;
using System.Net.Sockets;
using Autofac;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.Operations.Client;
using Lykke.Service.Operations.Contracts;
using Lykke.SettingsReader;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Lykke.Service.FixGateway.Tests.TradeSessionIntegration
{
    [TestFixture, Explicit]
    internal class OperationServiceFailedTest : ExternalServiceFailedBase
    {
        protected override void InitContainer(LocalSettingsReloadingManager<AppSettings> appSettings, ContainerBuilder builder)
        {
            base.InitContainer(appSettings, builder);
            var ocProxy = Substitute.For<IOperationsClient>();

            ocProxy.Complete(Arg.Any<Guid>()).ThrowsForAnyArgs(new SocketException());
            ocProxy.NewOrder(Arg.Any<Guid>(), Arg.Any<CreateNewOrderCommand>()).ThrowsForAnyArgs(new SocketException());

            builder.RegisterInstance(ocProxy)
                .As<IOperationsClient>();
        }
    }
}
