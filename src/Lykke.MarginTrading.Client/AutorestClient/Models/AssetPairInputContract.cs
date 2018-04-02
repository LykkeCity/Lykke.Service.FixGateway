// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.MarginTrading.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class AssetPairInputContract
    {
        /// <summary>
        /// Initializes a new instance of the AssetPairInputContract class.
        /// </summary>
        public AssetPairInputContract()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AssetPairInputContract class.
        /// </summary>
        /// <param name="matchingEngineMode">Possible values include:
        /// 'MarketMaker', 'Stp'</param>
        public AssetPairInputContract(int accuracy, MatchingEngineModeContract matchingEngineMode, double stpMultiplierMarkupBid, double stpMultiplierMarkupAsk, string name = default(string), string baseAssetId = default(string), string quoteAssetId = default(string), string legalEntity = default(string), string basePairId = default(string))
        {
            Name = name;
            BaseAssetId = baseAssetId;
            QuoteAssetId = quoteAssetId;
            Accuracy = accuracy;
            LegalEntity = legalEntity;
            BasePairId = basePairId;
            MatchingEngineMode = matchingEngineMode;
            StpMultiplierMarkupBid = stpMultiplierMarkupBid;
            StpMultiplierMarkupAsk = stpMultiplierMarkupAsk;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "baseAssetId")]
        public string BaseAssetId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "quoteAssetId")]
        public string QuoteAssetId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "accuracy")]
        public int Accuracy { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "legalEntity")]
        public string LegalEntity { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "basePairId")]
        public string BasePairId { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'MarketMaker', 'Stp'
        /// </summary>
        [JsonProperty(PropertyName = "matchingEngineMode")]
        public MatchingEngineModeContract MatchingEngineMode { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "stpMultiplierMarkupBid")]
        public double StpMultiplierMarkupBid { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "stpMultiplierMarkupAsk")]
        public double StpMultiplierMarkupAsk { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
        }
    }
}
