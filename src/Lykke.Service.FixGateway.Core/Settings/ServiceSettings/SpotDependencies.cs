using JetBrains.Annotations;
using Lykke.Service.FeeCalculator.Client;

namespace Lykke.Service.FixGateway.Core.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed class SpotDependencies
    {
        public AssetsServiceClient Assets { get; set; }
        public MatchingEngineSettings MatchingEngineClient { get; set; }
        public OperationsServiceClient OperationsServiceClient { get; set; }
        public FeeSettings FeeSettings { get; set; }
        public FeeCalculatorServiceClientSettings FeeCalculatorServiceClient { get; set; }
        public SpotRabbitMqConfiguration RabbitMq { get; set; }

    }
}
