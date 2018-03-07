using System;
using System.Threading.Tasks;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services;
using Lykke.Service.Operations.Client.AutorestClient;
using Lykke.SettingsReader;
using NUnit.Framework;
using StackExchange.Redis;
using CreateNewOrderCommand = Lykke.Service.Operations.Client.AutorestClient.Models.CreateNewOrderCommand;
using OperationStatus = Lykke.Service.Operations.Client.AutorestClient.Models.OperationStatus;

namespace Lykke.Service.FixGateway.Tests
{
    [TestFixture, Explicit]
    public class OperationsClientTest
    {
        private readonly string _serviceUrl = Environment.GetEnvironmentVariable("OperationServiceUrl");
        private ConnectionMultiplexer _multiplexer;

        [SetUp]
        public void SetUp()
        {
            var appSettings = new LocalSettingsReloadingManager<AppSettings>("appsettings.Development.json");
            _multiplexer = ConnectionMultiplexer.Connect(appSettings.CurrentValue.RedisSettings.Configuration);

        }

        [Test]
        public async Task ShouldCallOperationsService()
        {
            var ordId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var client = new OperationsAPI(new Uri(_serviceUrl));
            var result = await client.ApiOperationsNewOrderByIdPostAsync(ordId, new CreateNewOrderCommand
            {
                ClientOrderId = "fsdfsdf",
                WalletId = clientId
            });

            Assert.That(result, Is.Not.EqualTo(Guid.Empty));
            var res = await client.ApiOperationsByClientIdListByStatusGetAsync(clientId, OperationStatus.Created);

            Assert.That(res, Is.Not.Null);
            Assert.That(res, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task CancelAllActive()
        {
            var client = new OperationsAPI(new Uri(_serviceUrl));
            var allItems = await _multiplexer.GetDatabase().HashGetAllAsync(string.Format(ClientOrderIdProvider.KeyPrefix, Const.ClientId));

            foreach (var entry in allItems)
            {
                var id1 = entry.Name;
                var id2 = entry.Value;
                try
                {
                    await client.ApiOperationsCancelByIdPostAsync(Guid.Parse(id1));

                }
                catch {}

                try
                {
                    await client.ApiOperationsCancelByIdPostAsync(Guid.Parse(id2));
                }
                catch {}
               
            }

        }
    }
}
