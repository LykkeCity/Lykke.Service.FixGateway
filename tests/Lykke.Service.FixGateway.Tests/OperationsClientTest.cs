using System;
using System.Threading.Tasks;
using Lykke.Service.Operations.Client.AutorestClient;
using NUnit.Framework;
using CreateNewOrderCommand = Lykke.Service.Operations.Client.AutorestClient.Models.CreateNewOrderCommand;
using OperationStatus = Lykke.Service.Operations.Client.AutorestClient.Models.OperationStatus;

namespace Lykke.Service.FixGateway.Tests
{
    [TestFixture, Explicit]
    public class OperationsClientTest
    {
        private readonly string _serviceUrl = Environment.GetEnvironmentVariable("OperationServiceUrl");

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
    }
}
