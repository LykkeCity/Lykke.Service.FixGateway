using System;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.MatchingEngine.Connector.Abstractions.Models;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Services;
using Lykke.Service.FixGateway.Services.Mappings;
using NSubstitute;
using NUnit.Framework;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using SessionState = Lykke.Service.FixGateway.Services.SessionState;

namespace Lykke.Service.FixGateway.Tests.Spot
{
    [TestFixture]
    public class OrderCancelRequestHandlerTest
    {
        private OrderCancelRequestHandler _handler;
        private IClientOrderIdProvider _clientOrderIdProvider;
        private IMatchingEngineClient _matchingEngineClient;
        private IOrderCancelRequestValidator _validator;
        private IFixMessagesSender _messageSender;

        [SetUp]
        public void Setup()
        {
            _messageSender = Substitute.For<IFixMessagesSender>();
            var log = new LogToConsole();
            var sessState = new SessionState(new SessionID("", "", ""), log);

            _clientOrderIdProvider = Substitute.For<IClientOrderIdProvider>();
            _matchingEngineClient = Substitute.For<IMatchingEngineClient>();
            _validator = Substitute.For<IOrderCancelRequestValidator>();
            var mapper = new MapperConfiguration(c => c.AddProfile(new AutoMapperProfile())).CreateMapper();
            _handler = new OrderCancelRequestHandler(sessState, _messageSender, log, _clientOrderIdProvider, _matchingEngineClient, _validator, mapper);
        }

        [Test]
        public async Task ShouldCallMeClient()
        {
            _validator.ValidateAsync(Arg.Any<OrderCancelRequest>()).Returns(Task.FromResult(true));
            _clientOrderIdProvider.GetOrderIdByClientOrderId("").ReturnsForAnyArgs(Task.FromResult(Guid.NewGuid()));
            _matchingEngineClient.CancelLimitOrderAsync("").ReturnsForAnyArgs(Task.FromResult(new MeResponseModel { Status = MeStatusCodes.Ok }));

            var request = CreateRequest();
            await _handler.Handle(request);

            await _matchingEngineClient.ReceivedWithAnyArgs(1).CancelLimitOrderAsync("");
            _messageSender.Received(1).Send(Arg.Any<ExecutionReport>());

        }

        [TestCase(MeStatusCodes.AlreadyProcessed)]
        [TestCase(MeStatusCodes.NotFound)]
        [TestCase(MeStatusCodes.Runtime)]
        public async Task ShouldReturnOrderCancelReject(MeStatusCodes meErrorCode)
        {
            _validator.ValidateAsync(Arg.Any<OrderCancelRequest>()).Returns(Task.FromResult(true));
            _clientOrderIdProvider.GetOrderIdByClientOrderId("").ReturnsForAnyArgs(Task.FromResult(Guid.NewGuid()));
            _matchingEngineClient.CancelLimitOrderAsync("").ReturnsForAnyArgs(Task.FromResult(new MeResponseModel { Status = meErrorCode }));

            var request = CreateRequest();
            request.Header.SetField(new MsgSeqNum(1));
            request.Header.SetField(new MsgType("Something"));
            await _handler.Handle(request);

            _messageSender.Received(1).Send(Arg.Any<OrderCancelReject>());

        }

        [Test]
        public async Task ShouldReturnRejectIfMeUnavailable()
        {
            _validator.ValidateAsync(Arg.Any<OrderCancelRequest>()).Returns(Task.FromResult(true));
            _clientOrderIdProvider.GetOrderIdByClientOrderId("").ReturnsForAnyArgs(Task.FromResult(Guid.NewGuid()));

            var request = CreateRequest();
            request.Header.SetField(new MsgSeqNum(1));
            request.Header.SetField(new MsgType("Something"));
            await _handler.Handle(request);

            _messageSender.Received(1).Send(Arg.Any<Reject>());
        }

        private static OrderCancelRequest CreateRequest()
        {
            return new OrderCancelRequest(new OrigClOrdID("someClId"), new ClOrdID("someId"), new Symbol("USDEUR"), new Side(Side.BUY), new TransactTime(DateTime.Now));
        }
    }
}
