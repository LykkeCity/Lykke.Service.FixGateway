using System;
using System.Collections.Generic;
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
                var batch = db.CreateBatch();
                var tasks = new List<Task>
                {
                    batch.HashSetAsync(_key, clientOrderId, orderId.ToString()),
                    batch.HashSetAsync(_key, orderId.ToString(), clientOrderId),
                    batch.KeyExpireAsync(_key, _keyExpirationPeriod)
                };
                batch.Execute();
                await Task.WhenAll(tasks);
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
            var clientOrderId = await FindClientOrderIdByOrderIdAsync(orderId);
            await _operationsClient.Complete(orderId);
            var db = GetDatabase();
            if (!string.IsNullOrEmpty(clientOrderId))
            {
                await db.HashDeleteAsync(_key, clientOrderId);
            }

            await db.HashDeleteAsync(_key, orderId.ToString());
        }

        public async Task<string> FindClientOrderIdByOrderIdAsync(Guid orderId)
        {
            var db = GetDatabase();
            var clientOrderId = await db.HashGetAsync(_key, orderId.ToString());
            return clientOrderId;
        }

        public async Task<Guid> GetOrderIdByClientOrderId(string clientOrderId)
        {
            var db = GetDatabase();
            var ordId = await db.HashGetAsync(_key, clientOrderId);
            if (!ordId.HasValue)
            {
                throw new InvalidOperationException($"Unknown ClientOrderID {clientOrderId}");
            }
            return Guid.Parse(ordId);
        }

        public void Start()
        {
            var operations = _operationsClient.Get(_credentials.ClientId, OperationStatus.Created).GetAwaiter().GetResult();
            var db = GetDatabase();
            var tasks = new List<Task>();

            var batch = db.CreateBatch();

            tasks.Add(db.KeyDeleteAsync(_key));
            foreach (var operation in operations)
            {
                var clientOrderId = JsonConvert.DeserializeObject<NewOrderContext>(operation.ContextJson).ClientOrderId;
                tasks.Add(batch.HashSetAsync(_key, clientOrderId, operation.Id.ToString()));
                tasks.Add(batch.HashSetAsync(_key, operation.Id.ToString(), clientOrderId));
                tasks.Add(batch.KeyExpireAsync(_key, _keyExpirationPeriod));

            }
            batch.Execute();
            Task.WhenAll(tasks).GetAwaiter().GetResult();
        }
    }
}
