using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.FixGateway.Core.Settings.SlackNotifications;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    public sealed class AppSettings
    {
        public FixGatewaySettings FixGatewayService { get; set; }
        public AssetsServiceClient Assets { get; set; }
        public MatchingEngineSettings MatchingEngineClient { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public FeeCalculatorServiceClientSettings FeeCalculatorServiceClient { get; set; }
        public RedisSettings RedisSettings { get; set; }
        public OperationsServiceClient OperationsServiceClient { get; set; }
        public FeeSettings FeeSettings { get; set; }
    }
}
