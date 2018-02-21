namespace Lykke.Service.FixGateway.Core.Settings
{
    public sealed class RabbitMqLykkeConfiguration
    {
        public RabbitMqExchangeConfiguration IncomingOrderBooks { get; set; }
        public RabbitMqExchangeConfiguration IncomingMarketOrders { get; set; }
    }
}
