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

    public partial class InitDataBackendResponse
    {
        /// <summary>
        /// Initializes a new instance of the InitDataBackendResponse class.
        /// </summary>
        public InitDataBackendResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the InitDataBackendResponse class.
        /// </summary>
        public InitDataBackendResponse(bool isLive, IList<MarginTradingAccountBackendContract> accounts = default(IList<MarginTradingAccountBackendContract>), IDictionary<string, IList<AccountAssetPairModel>> accountAssetPairs = default(IDictionary<string, IList<AccountAssetPairModel>>))
        {
            Accounts = accounts;
            AccountAssetPairs = accountAssetPairs;
            IsLive = isLive;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "accounts")]
        public IList<MarginTradingAccountBackendContract> Accounts { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "accountAssetPairs")]
        public IDictionary<string, IList<AccountAssetPairModel>> AccountAssetPairs { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "isLive")]
        public bool IsLive { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Accounts != null)
            {
                foreach (var element in Accounts)
                {
                    if (element != null)
                    {
                        element.Validate();
                    }
                }
            }
            if (AccountAssetPairs != null)
            {
                foreach (var valueElement in AccountAssetPairs.Values)
                {
                    if (valueElement != null)
                    {
                        foreach (var element1 in valueElement)
                        {
                            if (element1 != null)
                            {
                                element1.Validate();
                            }
                        }
                    }
                }
            }
        }
    }
}
