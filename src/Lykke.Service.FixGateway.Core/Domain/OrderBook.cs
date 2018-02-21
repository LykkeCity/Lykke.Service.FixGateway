using System;
using System.Collections.Generic;

namespace Lykke.Service.FixGateway.Core.Domain
{
    public sealed class OrderBook
    {
        public string AssetPair { get; set; }
        public bool IsBuy { get; set; }
        public DateTime Timestamp { get; set; }
        public IReadOnlyList<VolumePrice> Prices { get; set; } = Array.Empty<VolumePrice>();
    }

    public sealed class VolumePrice
    {
        public double Volume { get; set; }
        public double Price { get; set; }
    }
}
