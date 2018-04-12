using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.MarginTrading.Client.AutorestClient;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services.Adapters;
using Lykke.Service.FixGateway.Services.Mappings;
using Lykke.SettingsReader;
using NUnit.Framework;

namespace Lykke.Service.FixGateway.Tests.MT
{
    [TestFixture]
    internal class MtAssetsServiceAdapterTest
    {

        private MtAssetsServiceAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            var settings = new LocalSettingsReloadingManager<AppSettings>("appsettings.Development.json");
            var mtSettings = settings.CurrentValue.FixGatewayService.MtDependencies.MarginTradingClientSettings;
            var mtClient = new MarginTradingApi(new Uri(mtSettings.ServiceUrl), mtSettings.ApiKey);
            var cred = settings.CurrentValue.FixGatewayService.Credentials;
            var mapper = new MapperConfiguration(cfg => cfg.AddProfile(new AutoMapperProfile())).CreateMapper();
            _adapter = new MtAssetsServiceAdapter(mtClient, cred, mapper, new LogToConsole());
        }

        [Test]
        public async Task ShouldReturnAllAssets()
        {
            var ass = await _adapter.GetAllAssetPairsAsync();
            Assert.That(ass, Is.Not.Empty);
        }

        [Test]
        public async Task ShouldReturnAssetIfExists()
        {
            var ass = await _adapter.GetAllAssetPairsAsync();

            var asset = _adapter.TryGetAssetPairAsync(ass.First().Id);

            Assert.That(asset, Is.Not.Null);
        }

        [Test]
        public async Task ShouldNotReturnAssetIfNotExists()
        {

            var myAss = "SomeAssId";
            var asset = await _adapter.TryGetAssetPairAsync(myAss);

            Assert.That(asset, Is.Null);
        }
    }
}
