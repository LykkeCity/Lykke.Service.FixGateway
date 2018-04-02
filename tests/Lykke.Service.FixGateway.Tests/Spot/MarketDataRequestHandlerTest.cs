using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Services;
using NSubstitute;
using NUnit.Framework;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests.Spot
{
    [TestFixture]
    public class MarketDataRequestHandlerTest
    {
        private MarketDataRequestHandler _handler;
        private IAssetsService _assetsService;
        private IFixMessagesSender _messagesSender;

        [SetUp]
        public void SetUp()
        {
            _assetsService = Substitute.For<IAssetsService>();
            _messagesSender = Substitute.For<IFixMessagesSender>();
            var messageProducer = Substitute.For<IObservable<OrderBook>>();

            var validator = new MarketDataRequestValidator(_assetsService, _messagesSender);
            _handler = new MarketDataRequestHandler(validator, messageProducer, _messagesSender, new LogToConsole());
        }

        [Test]
        public async Task ShouldRejectRequestsForDisabledAssetPairs()
        {
            IReadOnlyCollection<AssetPair> disabledPair = new[] { new AssetPair("_BTCUSD", "", "", 0) };

            _assetsService.GetAllAssetPairsAsync().ReturnsForAnyArgs(Task.FromResult(disabledPair));

            var request = FixMessagesFactory.CreateMarketDataRequest();
            await _handler.Handle(request);
            Thread.Sleep(500);
            _messagesSender.Received(1).Send(Arg.Any<MarketDataRequestReject>());
        }

        [Test]
        public async Task ShouldNotRejectRequestsForEnabledAssetPairs()
        {
            IReadOnlyCollection<AssetPair> disabledPair = new[] { new AssetPair("BTCUSD", "", "", 0) };

            _assetsService.GetAllAssetPairsAsync().ReturnsForAnyArgs(Task.FromResult(disabledPair));

            var request = FixMessagesFactory.CreateMarketDataRequest();
            await _handler.Handle(request);
            Thread.Sleep(2000); // Will wait for a new event from RabbitMq and never send a response
            _messagesSender.Received(0).Send(Arg.Any<MarketDataRequestReject>());
        }
    }
}
