
using System;
using NUnit.Framework;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests
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
            _fixClient.Start();
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
            var request = CreateMarketDataRequest("BTCUSD", depth: 1);
            _fixClient.Start();
            _fixClient.Send(request);

            for (int j = 0; j < 5; j++)
            {
                var resp = _fixClient.GetResponse<MarketDataSnapshotFullRefresh>();
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


        private static MarketDataRequest CreateMarketDataRequest(string assetPair = "BTCUSD", string id = "34", bool bid = true, bool ask = true, int depth = 0)
        {
            var request = new MarketDataRequest
            {
                MDReqID = new MDReqID(id),
                SubscriptionRequestType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES),
                MarketDepth = new MarketDepth(depth),
                NoMDEntryTypes = new NoMDEntryTypes(1),
                NoRelatedSym = new NoRelatedSym(1)
            };
            if (ask)
            {
                var typeGroup = new MarketDataRequest.NoMDEntryTypesGroup
                {
                    MDEntryType = new MDEntryType(MDEntryType.OFFER)
                };
                request.AddGroup(typeGroup);
            }

            if (bid)
            {
                var typeGroup = new MarketDataRequest.NoMDEntryTypesGroup
                {
                    MDEntryType = new MDEntryType(MDEntryType.BID)
                };
                request.AddGroup(typeGroup);
            }


            var symbolGroup = new MarketDataRequest.NoRelatedSymGroup
            {
                Symbol = new Symbol(assetPair)
            };

            request.AddGroup(symbolGroup);
            return request;
        }
    }
}
