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
        private readonly TimeSpan _keyExpirationPeriod = TimeSpan.FromDays(7);

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
                var db = GetDatabase();
                await db.KeyExpireAsync(_key, _keyExpirationPeriod);
                await db.HashSetAsync(_key, clientOrderId, orderId.ToString());
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
            return GetDatabase().HashExistsAsync(_key, clientOrderId);
        }

        public async Task RemoveCompletedAsync(Guid orderId)
        {
            var coid = await GetClientOrderIdByOrderIdAsync(orderId);
            await _operationsClient.Complete(orderId);
            await GetDatabase().HashDeleteAsync(_key, coid);
        }

        public async Task<string> GetClientOrderIdByOrderIdAsync(Guid orderId)
        {
            var context = await _operationsClient.Get(orderId);
            var coid = JsonConvert.DeserializeObject<NewOrderContext>(context.ContextJson).ClientOrderId;
            return coid;
        }

        public void Start()
        {
            var operations = _operationsClient.Get(_credentials.ClientId, OperationStatus.Created).GetAwaiter().GetResult().ToArray();
            var db = GetDatabase();
            db.KeyDeleteAsync(_key).GetAwaiter().GetResult();
            var batch = db.CreateBatch();
            foreach (var operation in operations)
            {
                var coid = JsonConvert.DeserializeObject<NewOrderContext>(operation.ContextJson).ClientOrderId;
                batch.KeyExpireAsync(_key, _keyExpirationPeriod);
                batch.HashSetAsync(_key, coid, operation.Id.ToString()); // Do not await here
            }
            batch.Execute();

        }
    }
}
