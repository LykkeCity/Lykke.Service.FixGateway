// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Lykke.MarginTrading.Client.AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class AccountDepositWithdrawResponse
    {
        /// <summary>
        /// Initializes a new instance of the AccountDepositWithdrawResponse
        /// class.
        /// </summary>
        public AccountDepositWithdrawResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AccountDepositWithdrawResponse
        /// class.
        /// </summary>
        public AccountDepositWithdrawResponse(string transactionId = default(string))
        {
            TransactionId = transactionId;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "transactionId")]
        public string TransactionId { get; set; }

    }
}
