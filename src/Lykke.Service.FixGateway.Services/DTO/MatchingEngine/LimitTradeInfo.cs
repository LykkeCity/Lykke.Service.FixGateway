using System;
using System.Collections.Generic;

namespace Lykke.Service.FixGateway.Services.DTO.MatchingEngine
{
    public sealed class LimitTradeInfo
    {
        public string ClientId { get; set; }
        public string Asset { get; set; }
        public double Volume { get; set; }
        public double? Price { get; set; }
        public DateTime Timestamp { get; set; }
        public string OppositeOrderId { get; set; }
        public string OppositeOrderExternalId { get; set; }
        public string OppositeAsset { get; set; }
        public string OppositeClientId { get; set; }
        public double OppositeVolume { get; set; }
        public FeeTransfer FeeTransfer { get; set; }
        public IReadOnlyCollection<Fee> Fees { get; set; }
    }
}
