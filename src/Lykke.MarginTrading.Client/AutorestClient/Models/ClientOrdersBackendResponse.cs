// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.MarginTrading.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class ClientOrdersBackendResponse
    {
        /// <summary>
        /// Initializes a new instance of the ClientOrdersBackendResponse
        /// class.
        /// </summary>
        public ClientOrdersBackendResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ClientOrdersBackendResponse
        /// class.
        /// </summary>
        public ClientOrdersBackendResponse(IList<OrderBackendContract> positions = default(IList<OrderBackendContract>), IList<OrderBackendContract> orders = default(IList<OrderBackendContract>))
        {
            Positions = positions;
            Orders = orders;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "positions")]
        public IList<OrderBackendContract> Positions { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "orders")]
        public IList<OrderBackendContract> Orders { get; set; }

    }
}
