using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lykke.MarginTrading.Client.AutorestClient.Models;
using Microsoft.Rest;

// ReSharper disable once CheckNamespace
namespace Lykke.MarginTrading.Client.AutorestClient
{
    public partial interface IMarginTradingApi
    {
        Task<HttpOperationResponse<IList<AssetPairBackendContract>>> ApiMtInitassetsPostWithHttpMessagesAsync(ClientIdBackendRequest request = default(ClientIdBackendRequest), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken));
    }

    public partial class MarginTradingApi
    {
        private readonly string _apiKey;

        /// <inheritdoc />
        public MarginTradingApi(System.Uri baseUri, string apiKey, params DelegatingHandler[] handlers) : this(baseUri, handlers)
        {
            _apiKey = apiKey;
        }

        /// <inheritdoc />
        public Task<HttpOperationResponse<IList<AssetPairBackendContract>>> ApiMtInitassetsPostWithHttpMessagesAsync(ClientIdBackendRequest request = default(ClientIdBackendRequest), Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ApiMtInitassetsPostWithHttpMessagesAsync(_apiKey, request, customHeaders, cancellationToken);
        }
    }

    public static partial class MarginTradingApiExtensions
    {
        /// <param name='operations'>
        /// The operations group for this extension method.
        /// </param>
        /// access token
        /// </param>
        /// <param name='request'>
        /// </param>
        /// <param name='cancellationToken'>
        /// The cancellation token.
        /// </param>
        public static async Task<IList<AssetPairBackendContract>> GetAssetsAsync(this IMarginTradingApi operations, ClientIdBackendRequest request = default(ClientIdBackendRequest), CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var result = await operations.ApiMtInitassetsPostWithHttpMessagesAsync(request, null, cancellationToken).ConfigureAwait(false))
            {
                return result.Body;
            }
        }
    }
}
