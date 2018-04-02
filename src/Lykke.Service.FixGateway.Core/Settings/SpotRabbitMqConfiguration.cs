using JetBrains.Annotations;

namespace Lykke.Service.FixGateway.Core.Settings
{
    [UsedImplicitly]
    public sealed class SpotRabbitMqConfiguration
    {
        public RabbitMqExchangeConfiguration IncomingOrderBooks { get; set; }
        public RabbitMqExchangeConfiguration IncomingMarketOrders { get; set; }
        public RabbitMqExchangeConfiguration IncomingLimitOrders { get; set; }
    }
}
