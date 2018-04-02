// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.MarginTrading.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class NewOrderBackendContract
    {
        /// <summary>
        /// Initializes a new instance of the NewOrderBackendContract class.
        /// </summary>
        public NewOrderBackendContract()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the NewOrderBackendContract class.
        /// </summary>
        /// <param name="fillType">Possible values include: 'FillOrKill',
        /// 'PartialFill'</param>
        public NewOrderBackendContract(double volume, OrderFillTypeContract fillType, string accountId = default(string), string instrument = default(string), double? expectedOpenPrice = default(double?), double? takeProfit = default(double?), double? stopLoss = default(double?))
        {
            AccountId = accountId;
            Instrument = instrument;
            ExpectedOpenPrice = expectedOpenPrice;
            Volume = volume;
            TakeProfit = takeProfit;
            StopLoss = stopLoss;
            FillType = fillType;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "instrument")]
        public string Instrument { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "expectedOpenPrice")]
        public double? ExpectedOpenPrice { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "volume")]
        public double Volume { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "takeProfit")]
        public double? TakeProfit { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "stopLoss")]
        public double? StopLoss { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'FillOrKill', 'PartialFill'
        /// </summary>
        [JsonProperty(PropertyName = "fillType")]
        public OrderFillTypeContract FillType { get; set; }

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
