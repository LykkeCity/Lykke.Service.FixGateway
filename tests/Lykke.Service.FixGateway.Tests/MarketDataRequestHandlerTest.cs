using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.FixGateway.Core.Domain;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Services;
using NSubstitute;
using NUnit.Framework;
using QuickFix;
using QuickFix.FIX44;
using SessionState = Lykke.Service.FixGateway.Services.SessionState;

namespace Lykke.Service.FixGateway.Tests
{
    [TestFixture]
    public class MarketDataRequestHandlerTest
    {
        private MarketDataRequestHandler _handler;
        private IAssetsServiceWithCache _assetsService;
        private IObservable<OrderBook> _messageProducer;
        private IFixMessagesSender _messagesSender;
        private SessionID _sessionID;

        [SetUp]
        public void SetUp()
        {
            _sessionID = new SessionID("", "", "");
            _assetsService = Substitute.For<IAssetsServiceWithCache>();
            _messagesSender = Substitute.For<IFixMessagesSender>();
            _messageProducer = Substitute.For<IObservable<OrderBook>>();
            var sessionState = new SessionState(_sessionID);
            _handler = new MarketDataRequestHandler(_assetsService, _messageProducer, sessionState, _messagesSender, new LogToConsole());
        }

        [Test]
        public void ShouldRejectRequestsForDisabledAssetPairs()
        {
            IReadOnlyCollection<AssetPair> disabledPair = new[] { new AssetPair(10, 10, true, 10, 10) };

            _assetsService.GetAllAssetPairsAsync().ReturnsForAnyArgs(Task.FromResult(disabledPair));

            var request = FixMessagesFactory.CreateMarketDataRequest();
            _handler.Handle(request);
            Thread.Sleep(500);
            _messagesSender.Received(1).Send(Arg.Any<MarketDataRequestReject>(), _sessionID);
        }

        [Test]
        public void ShouldNotRejectRequestsForEnabledAssetPairs()
        {
            IReadOnlyCollection<AssetPair> disabledPair = new[] { new AssetPair(10, 10, false, 10, 10, id: "BTCUSD") };

            _assetsService.GetAllAssetPairsAsync().ReturnsForAnyArgs(Task.FromResult(disabledPair));

            var request = FixMessagesFactory.CreateMarketDataRequest();
            _handler.Handle(request);
            Thread.Sleep(2000); // Will wait for a new event from RabbitMq and never send a response
            _messagesSender.Received(0).Send(Arg.Any<MarketDataRequestReject>(), _sessionID);
        }
    }
}
