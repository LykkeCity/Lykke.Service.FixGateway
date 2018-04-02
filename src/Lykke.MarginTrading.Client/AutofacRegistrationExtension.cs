using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.MarginTrading.Client.AutorestClient;

namespace Lykke.MarginTrading.Client
{
    /// <summary>
    /// 
    /// </summary>
    [UsedImplicitly]
    public static class AutofacRegistrationExtension
    {
        /// <summary>
        /// Registers a MT client
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="url"></param>
        /// <param name="setApiKey"></param>
        public static void RegisterMarginTradingClient(this ContainerBuilder builder, [NotNull] string url, string setApiKey)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            builder.RegisterType<MarginTradingApi>()
                .WithParameter(TypedParameter.From(new Uri(url)))
                .As<IMarginTradingApi>();
        }
    }
}
