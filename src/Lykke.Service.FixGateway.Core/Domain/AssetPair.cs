namespace Lykke.Service.FixGateway.Core.Domain
{
    public sealed class AssetPair
    {
        public AssetPair(string id, string baseAssetId, string quoteAssetId, int accuracy)
        {
            Id = id;
            BaseAssetId = baseAssetId;
            QuoteAssetId = quoteAssetId;
            Accuracy = accuracy;
        }

        public string Id { get; }

        public string BaseAssetId { get; }

        public string QuoteAssetId { get; }

        public int Accuracy { get; }
    }
}
