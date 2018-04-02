// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.MarginTrading.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class MatchingEngineRoute
    {
        /// <summary>
        /// Initializes a new instance of the MatchingEngineRoute class.
        /// </summary>
        public MatchingEngineRoute()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MatchingEngineRoute class.
        /// </summary>
        /// <param name="type">Possible values include: 'Buy', 'Sell'</param>
        public MatchingEngineRoute(int rank, string id = default(string), string tradingConditionId = default(string), string clientId = default(string), string instrument = default(string), OrderDirection? type = default(OrderDirection?), string matchingEngineId = default(string), string asset = default(string), string riskSystemLimitType = default(string), string riskSystemMetricType = default(string))
        {
            Id = id;
            Rank = rank;
            TradingConditionId = tradingConditionId;
            ClientId = clientId;
            Instrument = instrument;
            Type = type;
            MatchingEngineId = matchingEngineId;
            Asset = asset;
            RiskSystemLimitType = riskSystemLimitType;
            RiskSystemMetricType = riskSystemMetricType;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "rank")]
        public int Rank { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "tradingConditionId")]
        public string TradingConditionId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "instrument")]
        public string Instrument { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Buy', 'Sell'
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public OrderDirection? Type { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "matchingEngineId")]
        public string MatchingEngineId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "asset")]
        public string Asset { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "riskSystemLimitType")]
        public string RiskSystemLimitType { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "riskSystemMetricType")]
        public string RiskSystemMetricType { get; set; }

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
