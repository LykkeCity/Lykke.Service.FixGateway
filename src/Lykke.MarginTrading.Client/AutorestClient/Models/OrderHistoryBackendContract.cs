// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.MarginTrading.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class OrderHistoryBackendContract
    {
        /// <summary>
        /// Initializes a new instance of the OrderHistoryBackendContract
        /// class.
        /// </summary>
        public OrderHistoryBackendContract()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the OrderHistoryBackendContract
        /// class.
        /// </summary>
        /// <param name="type">Possible values include: 'Buy', 'Sell'</param>
        /// <param name="status">Possible values include:
        /// 'WaitingForExecution', 'Active', 'Closed', 'Rejected',
        /// 'Closing'</param>
        /// <param name="closeReason">Possible values include: 'None', 'Close',
        /// 'StopLoss', 'TakeProfit', 'StopOut', 'Canceled',
        /// 'CanceledBySystem', 'ClosedByBroker'</param>
        /// <param name="matchingEngineMode">Possible values include:
        /// 'MarketMaker', 'Stp'</param>
        public OrderHistoryBackendContract(int assetAccuracy, OrderDirectionContract type, OrderStatusContract status, OrderCloseReasonContract closeReason, double openPrice, double closePrice, double volume, double totalPnl, double pnl, double interestRateSwap, double commissionLot, double openCommission, double closeCommission, double openPriceEquivalent, double closePriceEquivalent, MatchingEngineModeContract matchingEngineMode, string id = default(string), string accountId = default(string), string instrument = default(string), System.DateTime? openDate = default(System.DateTime?), System.DateTime? closeDate = default(System.DateTime?), double? takeProfit = default(double?), double? stopLoss = default(double?), string equivalentAsset = default(string), string openExternalOrderId = default(string), string openExternalProviderId = default(string), string closeExternalOrderId = default(string), string closeExternalProviderId = default(string), string legalEntity = default(string))
        {
            Id = id;
            AccountId = accountId;
            Instrument = instrument;
            AssetAccuracy = assetAccuracy;
            Type = type;
            Status = status;
            CloseReason = closeReason;
            OpenDate = openDate;
            CloseDate = closeDate;
            OpenPrice = openPrice;
            ClosePrice = closePrice;
            Volume = volume;
            TakeProfit = takeProfit;
            StopLoss = stopLoss;
            TotalPnl = totalPnl;
            Pnl = pnl;
            InterestRateSwap = interestRateSwap;
            CommissionLot = commissionLot;
            OpenCommission = openCommission;
            CloseCommission = closeCommission;
            EquivalentAsset = equivalentAsset;
            OpenPriceEquivalent = openPriceEquivalent;
            ClosePriceEquivalent = closePriceEquivalent;
            OpenExternalOrderId = openExternalOrderId;
            OpenExternalProviderId = openExternalProviderId;
            CloseExternalOrderId = closeExternalOrderId;
            CloseExternalProviderId = closeExternalProviderId;
            LegalEntity = legalEntity;
            MatchingEngineMode = matchingEngineMode;
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
        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "instrument")]
        public string Instrument { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "assetAccuracy")]
        public int AssetAccuracy { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Buy', 'Sell'
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public OrderDirectionContract Type { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'WaitingForExecution',
        /// 'Active', 'Closed', 'Rejected', 'Closing'
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public OrderStatusContract Status { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'None', 'Close', 'StopLoss',
        /// 'TakeProfit', 'StopOut', 'Canceled', 'CanceledBySystem',
        /// 'ClosedByBroker'
        /// </summary>
        [JsonProperty(PropertyName = "closeReason")]
        public OrderCloseReasonContract CloseReason { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "openDate")]
        public System.DateTime? OpenDate { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "closeDate")]
        public System.DateTime? CloseDate { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "openPrice")]
        public double OpenPrice { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "closePrice")]
        public double ClosePrice { get; set; }

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
        /// </summary>
        [JsonProperty(PropertyName = "totalPnl")]
        public double TotalPnl { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "pnl")]
        public double Pnl { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "interestRateSwap")]
        public double InterestRateSwap { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "commissionLot")]
        public double CommissionLot { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "openCommission")]
        public double OpenCommission { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "closeCommission")]
        public double CloseCommission { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "equivalentAsset")]
        public string EquivalentAsset { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "openPriceEquivalent")]
        public double OpenPriceEquivalent { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "closePriceEquivalent")]
        public double ClosePriceEquivalent { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "openExternalOrderId")]
        public string OpenExternalOrderId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "openExternalProviderId")]
        public string OpenExternalProviderId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "closeExternalOrderId")]
        public string CloseExternalOrderId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "closeExternalProviderId")]
        public string CloseExternalProviderId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "legalEntity")]
        public string LegalEntity { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'MarketMaker', 'Stp'
        /// </summary>
        [JsonProperty(PropertyName = "matchingEngineMode")]
        public MatchingEngineModeContract MatchingEngineMode { get; set; }

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
