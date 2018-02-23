using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Contracts.Operations;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services;
using Lykke.Service.Operations.Client;
using Lykke.Service.Operations.Contracts;
using Lykke.SettingsReader;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using StackExchange.Redis;

namespace Lykke.Service.FixGateway.Tests
{
    [TestFixture, Explicit]
    internal class ClientOrderIdProviderTest
    {
        private ClientOrderIdProvider _clientOrderIdProvider;
        private IOperationsClient _operationsClient;
        private ConnectionMultiplexer _multiplexer;
        private readonly Guid _walletId = new Guid("{E2FAA82D-6BD6-4E9D-A14E-8E83FB32892C}");
        private readonly Guid _orderId = new Guid("{E2FAA82D-6BD6-4E9D-A14E-8E83FB32892D}");
        private readonly string _clientOrderId = "SomeClientId" + Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            var appSettings = new LocalSettingsReloadingManager<AppSettings>("appsettings.Development.json");
            _operationsClient = Substitute.For<IOperationsClient>();
            _operationsClient.NewOrder(Arg.Any<Guid>(), Arg.Any<CreateNewOrderCommand>()).Returns(Task.FromResult(_orderId));

            _multiplexer = ConnectionMultiplexer.Connect(appSettings.CurrentValue.RedisSettings.Configuration);
            var credentials = new Credentials { ClientId = _walletId };
            _clientOrderIdProvider = new ClientOrderIdProvider(_operationsClient, _multiplexer, credentials);
            _multiplexer.GetDatabase().KeyDelete(string.Format(ClientOrderIdProvider.KeyPrefix, _walletId));
        }

        [Test]
        public async Task ShouldAddNewClientOrderId()
        {
            await _clientOrderIdProvider.RegisterNewOrderAsync(_orderId, _clientOrderId);

            var set = await _multiplexer.GetDatabase().HashGetAsync(string.Format(ClientOrderIdProvider.KeyPrefix, _walletId), _clientOrderId);
            Assert.That(set.HasValue, Is.True);
            Assert.That(set.ToString(), Is.EqualTo(_orderId.ToString()));
        }

        [Test]
        public async Task StartShouldClearClientOrderIds()
        {
            _operationsClient.Get(Arg.Any<Guid>(), OperationStatus.Created).Returns(Task.FromResult(Enumerable.Empty<OperationModel>()));

            await _multiplexer.GetDatabase().HashSetAsync(string.Format(ClientOrderIdProvider.KeyPrefix, _walletId), new[] { new HashEntry("Name", "Value") });

            var set = await _multiplexer.GetDatabase().KeyExistsAsync(string.Format(ClientOrderIdProvider.KeyPrefix, _walletId));
            Assert.That(set, Is.True);

            _clientOrderIdProvider.Start();

            set = await _multiplexer.GetDatabase().KeyExistsAsync(string.Format(ClientOrderIdProvider.KeyPrefix, _walletId));
            Assert.That(set, Is.False);
        }

        [Test]
        public async Task StartShouldAddClientOrderIdsFromDb()
        {
            var ret = new[] { new OperationModel
            {
                Id = _orderId,
                ContextJson = JsonConvert.SerializeObject(new NewOrderContext
                                                {
                                                    ClientOrderId = _clientOrderId
                                                }
                )
            }
            };

            _operationsClient.Get(Arg.Any<Guid>(), OperationStatus.Created).Returns(Task.FromResult(ret.AsEnumerable()));

            _clientOrderIdProvider.Start();

            var set = await _multiplexer.GetDatabase().HashGetAsync(string.Format(ClientOrderIdProvider.KeyPrefix, _walletId), _clientOrderId);
            Assert.That(set, Is.Not.Null);
            Assert.That(set.ToString(), Is.EqualTo(_orderId.ToString()));
        }

        [Test]
        public async Task ShouldReturnClientOrderIdFromCache()
        {

            await _clientOrderIdProvider.RegisterNewOrderAsync(_orderId, _clientOrderId);

            var actual = await _clientOrderIdProvider.TryGetClientOrderIdByOrderIdAsync(_orderId);

            Assert.That(actual.clientOrderId, Is.EqualTo(_clientOrderId));
        }

        [Test]
        public async Task ShouldDeleteCompletedFromCache()
        {
            await _clientOrderIdProvider.RegisterNewOrderAsync(_orderId, _clientOrderId);


            await _clientOrderIdProvider.RemoveCompletedAsync(_orderId);


            var set1 = await _multiplexer.GetDatabase().HashExistsAsync(string.Format(ClientOrderIdProvider.KeyPrefix, _walletId), _clientOrderId);
            var set2 = await _multiplexer.GetDatabase().HashExistsAsync(string.Format(ClientOrderIdProvider.KeyPrefix, _walletId), _orderId.ToString());
            Assert.That(set1, Is.False);
            Assert.That(set2, Is.False);
        }


        [Test]
        public async Task PerformanceTest()
        {
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 100; i++)
            {
                await _clientOrderIdProvider.RegisterNewOrderAsync(_orderId, _clientOrderId + i);
            }
            Console.WriteLine($"Inserted 100 orders per {sw.ElapsedMilliseconds} ms {sw.ElapsedMilliseconds / 100d} ms per element");
        }
    }
}
