// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.MarginTrading.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class AccountHistoryItemBackend
    {
        /// <summary>
        /// Initializes a new instance of the AccountHistoryItemBackend class.
        /// </summary>
        public AccountHistoryItemBackend()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AccountHistoryItemBackend class.
        /// </summary>
        public AccountHistoryItemBackend(System.DateTime date, AccountHistoryBackendContract account = default(AccountHistoryBackendContract), OrderHistoryBackendContract position = default(OrderHistoryBackendContract))
        {
            Date = date;
            Account = account;
            Position = position;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "date")]
        public System.DateTime Date { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "account")]
        public AccountHistoryBackendContract Account { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "position")]
        public OrderHistoryBackendContract Position { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Account != null)
            {
                Account.Validate();
            }
            if (Position != null)
            {
                Position.Validate();
            }
        }
    }
}
