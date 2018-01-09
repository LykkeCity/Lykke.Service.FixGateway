using Lykke.MatchingEngine.Connector.Abstractions.Models;

namespace Lykke.Service.FixGateway.Services.DTO.MatchingEngine
{
    public sealed class LimitOrderFeeInstruction
    {
        public FeeType Type { get; set; }
        public FeeSizeType? TakerSizeType { get; set; }
        public FeeSizeType? MakerSizeType { get; set; }
        public double? MakerSize { get; set; }
        public double? TakerSize { get; set; }
        public string SourceClientId { get; set; }
        public string TargetClientId { get; set; }
    }
}