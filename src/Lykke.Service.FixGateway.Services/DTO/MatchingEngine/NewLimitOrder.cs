using System;
using System.Collections.Generic;
using Lykke.MatchingEngine.Connector.Models;

namespace Lykke.Service.FixGateway.Services.DTO.MatchingEngine
{
    public sealed class NewLimitOrder
    {
        public double? Price { get; set; }
        public DateTime? LastMatchTime { get; set; }
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public string AssetPairId { get; set; }
        public string ClientId { get; set; }
        public double Volume { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime Registered { get; set; }
        public double RemainingVolume { get; set; }
        public double? ReservedLimitVolume { get;set; }
        public IReadOnlyCollection<NewFeeInstruction> Fees { get; set; }
        public LimitOrderFeeInstruction Fee { get;set; }
    }
}
