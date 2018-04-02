using System;
using NUnit.Framework;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests.Spot
{
    [TestFixture, Explicit]
    public class QuoteSessionIntegrationTest
    {
        private FixClient _fixClient;
        private readonly string _uri =  Environment.GetEnvironmentVariable("ServiceUrl");

        [SetUp]
        public void SetUp()
        {
            _fixClient = new FixClient(uri: _uri);
        }

        [TearDown]
        public void TearDown()
        {
            _fixClient?.Stop();
        }

        [Test]
        public void GetAssetPairList()
        {
            var request = new SecurityListRequest
            {
                SecurityReqID = new SecurityReqID("42"),
                SecurityListRequestType = new SecurityListRequestType(SecurityListRequestType.SYMBOL)
            };
            _fixClient.Init();
            _fixClient.Send(request);
            var resp = _fixClient.GetResponse<SecurityList>();

            Assert.That(resp, Is.Not.Null);
            for (var i = 1; i <= resp.NoRelatedSym.Obj; i++)
            {
                var symb = ((SecurityList.NoRelatedSymGroup)resp.GetGroup(i, new SecurityList.NoRelatedSymGroup())).Symbol.Obj;
                Console.WriteLine(symb);
            }
        }

        [Test]
        public void SubscribeOnOrderBooks()
        {
            var request = FixMessagesFactory.CreateMarketDataRequest("ETHBTC", depth: 1);
            _fixClient.Init();
            _fixClient.Send(request);

            for (int j = 0; j < 5; j++)
            {
                var resp = _fixClient.GetResponse<MarketDataSnapshotFullRefresh>(10000);
                Assert.That(resp, Is.Not.Null);

                var subId = resp.MDReqID.Obj;
                var symbol = resp.Symbol.Obj;
                for (var i = 1; i <= resp.NoMDEntries.Obj; i++)
                {
                    var symb = ((MarketDataSnapshotFullRefresh.NoMDEntriesGroup)resp.GetGroup(i, new MarketDataSnapshotFullRefresh.NoMDEntriesGroup()));
                    var side = symb.MDEntryType.Obj == MDEntryType.OFFER ? "ask" : "bid";
                    Console.WriteLine($"{subId} {symbol} {side} {symb.MDEntryPx} {symb.MDEntrySize.Obj}");
                }
                Console.WriteLine();
            }
            _fixClient.Stop();
        }
    }
}
