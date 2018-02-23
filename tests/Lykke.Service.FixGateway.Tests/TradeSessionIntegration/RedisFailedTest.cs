using System;
using System.Net.Sockets;
using Autofac;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace Lykke.Service.FixGateway.Tests.TradeSessionIntegration
{
    [TestFixture, Explicit]
    internal class RedisFailedTest : ExternalServiceFailedBase
    {
        protected override void InitContainer(LocalSettingsReloadingManager<AppSettings> appSettings, ContainerBuilder builder)
        {
            base.InitContainer(appSettings, builder);
            var cop = Substitute.For<IClientOrderIdProvider>();
            cop.RemoveCompletedAsync(Arg.Any<Guid>()).ThrowsForAnyArgs(new SocketException());
            cop.CheckExistsAsync(Arg.Any<string>()).ThrowsForAnyArgs(new SocketException());
            cop.TryGetClientOrderIdByOrderIdAsync(Arg.Any<Guid>()).ThrowsForAnyArgs(new SocketException());
            cop.RegisterNewOrderAsync(Arg.Any<Guid>(), "").ThrowsForAnyArgs(new SocketException());

            builder.RegisterInstance(cop)
                .As<IClientOrderIdProvider>();
        }
    }
}
