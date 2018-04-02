// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.MarginTrading.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class OrderbooksBackendResponse
    {
        /// <summary>
        /// Initializes a new instance of the OrderbooksBackendResponse class.
        /// </summary>
        public OrderbooksBackendResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the OrderbooksBackendResponse class.
        /// </summary>
        public OrderbooksBackendResponse(OrderBookBackendContract orderbook = default(OrderBookBackendContract))
        {
            Orderbook = orderbook;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "orderbook")]
        public OrderBookBackendContract Orderbook { get; set; }

    }
}
