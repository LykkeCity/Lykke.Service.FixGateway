using Lykke.MatchingEngine.Connector.Abstractions.Models;

namespace Lykke.Service.FixGateway.Services.DTO.MatchingEngine
{
    public sealed class FeeInstruction
    {
        public FeeType Type { get; set; }
        public FeeSizeType SizeType { get; set; }
        public string SourceClientId { get; set; }
        public string TargetClientId { get; set; }

    }
}