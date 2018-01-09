using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Contracts.Operations;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.Operations.Client;
using Lykke.Service.Operations.Contracts;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Lykke.Service.FixGateway.Services
{
    public sealed class ClientOrderIdProvider : IClientOrderIdProvider
    {
        private readonly IOperationsClient _operationsClient;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly Credentials _credentials;
        private readonly RedisKey _key;
        public const string KeyPrefix = "FixGateway:WalletId:{0}";

        public ClientOrderIdProvider(IOperationsClient operationsClient, IConnectionMultiplexer connectionMultiplexer, Credentials credentials)
        {
            _operationsClient = operationsClient;
            _connectionMultiplexer = connectionMultiplexer;
            _credentials = credentials;
            _key = string.Format(KeyPrefix, _credentials.ClientId);
        }

        public async Task RegisterNewOrderAsync(Guid orderId, string clientOrderId)
        {


            await _operationsClient.NewOrder(orderId, new CreateNewOrderCommand
            {
                ClientOrderId = clientOrderId,
                WalletId = _credentials.ClientId
            });

            try
            {
                if (!await GetDatabase().SetAddAsync(_key, clientOrderId))
                {
                    throw new InvalidOperationException(@"Duplicate ClOrdId");
                }
            }
            catch (Exception)
            {
                await _operationsClient.Cancel(orderId);
                throw;
            }
        }

        private IDatabase GetDatabase()
        {
            return _connectionMultiplexer.GetDatabase();
        }

        public Task<bool> CheckExistsAsync(string clientOrderId)
        {
            return GetDatabase().SetContainsAsync(_key, clientOrderId);
        }

        public async Task RemoveCompletedAsync(Guid orderId)
        {
            var coid = await GetClientOrderIdByOrderIdAsync(orderId);
            await _operationsClient.Complete(orderId);
            await GetDatabase().SetRemoveAsync(_key, coid);
        }

        public async Task<string> GetClientOrderIdByOrderIdAsync(Guid orderId)
        {
            var context = await _operationsClient.Get(orderId);
            var coid = JsonConvert.DeserializeObject<NewOrderContext>(context.Context).ClientOrderId;
            return coid;
        }

        public void Start()
        {
            var operations = _operationsClient.Get(_credentials.ClientId, OperationStatus.Created).GetAwaiter().GetResult().ToArray();

            var transaction = GetDatabase().CreateTransaction();
            transaction.KeyDeleteAsync(_key); // Do not await here
            foreach (var operation in operations)
            {
                var coid = JsonConvert.DeserializeObject<NewOrderContext>(operation.Context).ClientOrderId;
                transaction.SetAddAsync(_key, coid); // and here also
            }
            transaction.Execute();

        }
    }
}
