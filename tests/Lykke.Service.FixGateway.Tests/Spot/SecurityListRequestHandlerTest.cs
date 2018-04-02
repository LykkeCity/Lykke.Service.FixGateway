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
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests.Spot
{
    [TestFixture]
    public class SecurityListRequestHandlerTest
    {
        private SecurityListRequestHandler _handler;
        private ISecurityListRequestValidator _validator;
        private IFixMessagesSender _messageSender;
        private IAssetsService _assetsService;

        [SetUp]
        public void Setup()
        {
            _messageSender = Substitute.For<IFixMessagesSender>();
            _assetsService = Substitute.For<IAssetsService>();
            var log = new LogToConsole();

            _validator = new SecurityListRequestValidator(_messageSender);
            _handler = new SecurityListRequestHandler(_assetsService, _messageSender, _validator, log);
        }

        [Test]
        public async Task ShouldReturnSecurityList()
        {
            var request = new SecurityListRequest(new SecurityReqID("Id"), new SecurityListRequestType(SecurityListRequestType.SYMBOL));
            _assetsService.GetAllAssetPairsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult((IReadOnlyCollection<AssetPair>)new[] { new AssetPair("USDEUR", "USD", "EUR", 5) }));
            await _handler.Handle(request);
            _messageSender.Received(1).Send(Arg.Is<SecurityList>(r =>
                r.SecurityRequestResult.Obj == SecurityRequestResult.VALID_REQUEST
                && r.NoRelatedSym.Obj > 0));
        }

        [Test]
        public async Task ShouldReturnSecurityListWithReject()
        {
            var request = new SecurityListRequest(new SecurityReqID("Id"), new SecurityListRequestType(SecurityListRequestType.ALL_SECURITIES));
            await _handler.Handle(request);
            _messageSender.Received(1).Send(Arg.Is<SecurityList>(r => r.SecurityRequestResult.Obj == SecurityRequestResult.INVALID_OR_UNSUPPORTED_REQUEST));
        }

        [Test]
        public async Task ShouldReturnSecurityListWithRejectIfAssetsServiceUnavailable()
        {

            var request = new SecurityListRequest(new SecurityReqID("Id"), new SecurityListRequestType(SecurityListRequestType.SYMBOL));
            request.Header.SetField(new MsgSeqNum(1));
            request.Header.SetField(new MsgType("Something"));
            _assetsService.GetAllAssetPairsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException<IReadOnlyCollection<AssetPair>>(new InvalidOperationException()));

            await _handler.Handle(request);
            _messageSender.Received(1).Send(Arg.Any<Reject>());
        }
    }
}
