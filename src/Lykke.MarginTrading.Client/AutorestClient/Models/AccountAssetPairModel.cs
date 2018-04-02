// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.MarginTrading.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class AccountAssetPairModel
    {
        /// <summary>
        /// Initializes a new instance of the AccountAssetPairModel class.
        /// </summary>
        public AccountAssetPairModel()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AccountAssetPairModel class.
        /// </summary>
        public AccountAssetPairModel(int leverageInit, int leverageMaintenance, double swapLong, double swapShort, double overnightSwapLong, double overnightSwapShort, double commissionLong, double commissionShort, double commissionLot, double deltaBid, double deltaAsk, double dealLimit, double positionLimit, string tradingConditionId = default(string), string baseAssetId = default(string), string instrument = default(string))
        {
            TradingConditionId = tradingConditionId;
            BaseAssetId = baseAssetId;
            Instrument = instrument;
            LeverageInit = leverageInit;
            LeverageMaintenance = leverageMaintenance;
            SwapLong = swapLong;
            SwapShort = swapShort;
            OvernightSwapLong = overnightSwapLong;
            OvernightSwapShort = overnightSwapShort;
            CommissionLong = commissionLong;
            CommissionShort = commissionShort;
            CommissionLot = commissionLot;
            DeltaBid = deltaBid;
            DeltaAsk = deltaAsk;
            DealLimit = dealLimit;
            PositionLimit = positionLimit;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "tradingConditionId")]
        public string TradingConditionId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "baseAssetId")]
        public string BaseAssetId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "instrument")]
        public string Instrument { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "leverageInit")]
        public int LeverageInit { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "leverageMaintenance")]
        public int LeverageMaintenance { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "swapLong")]
        public double SwapLong { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "swapShort")]
        public double SwapShort { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "overnightSwapLong")]
        public double OvernightSwapLong { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "overnightSwapShort")]
        public double OvernightSwapShort { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "commissionLong")]
        public double CommissionLong { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "commissionShort")]
        public double CommissionShort { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "commissionLot")]
        public double CommissionLot { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "deltaBid")]
        public double DeltaBid { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "deltaAsk")]
        public double DeltaAsk { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "dealLimit")]
        public double DealLimit { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "positionLimit")]
        public double PositionLimit { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            //Nothing to validate
        }
    }
}
