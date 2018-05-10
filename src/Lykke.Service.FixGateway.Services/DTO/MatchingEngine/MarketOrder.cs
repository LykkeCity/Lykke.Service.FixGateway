using System;
using System.Collections.Generic;
using Lykke.MatchingEngine.Connector.Models;

namespace Lykke.Service.FixGateway.Services.DTO.MatchingEngine
{
    public sealed class MarketOrder
    {
        public string Id { get; set; }
        public string Uid { get;set; }
        public string AssetPairId { get; set; }
        public string ClientId { get; set; }
        public double Volume { get; set; }
        public double? Price { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ExternalId { get; set; }
        public DateTime Registered { get; set; }
        public DateTime? MatchedAt { get; set; }
        public bool Straight { get; set; }
        public double? ReservedLimitVolume { get;set; }
        public IReadOnlyCollection<NewFeeInstruction> Fees { get; set; }
        public LimitOrderFeeInstruction Fee { get;set; }
    }
}
