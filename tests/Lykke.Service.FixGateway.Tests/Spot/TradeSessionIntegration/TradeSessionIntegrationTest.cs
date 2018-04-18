using System;
using NUnit.Framework;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests.Spot.TradeSessionIntegration
{
    [TestFixture("LYKKE_T", "SENDER_T", null, 12357)]
    [TestFixture(Const.TargetCompId, Const.SenderCompId, Const.Uri, Const.Port)]
    [Explicit("Only for manual testing on a dev machine")]
    internal class TradeSessionLocalIntegrationTest : TradeSessionIntegrationBase
    {
        private readonly string _targetCompId;
        private readonly string _senderCompId;
        private readonly string _uri;
        private readonly int _port;



        public TradeSessionLocalIntegrationTest(string targetCompId, string senderCompId, string uri, int port)
        {
            _targetCompId = targetCompId;
            _senderCompId = senderCompId;
            _uri = uri ?? Environment.GetEnvironmentVariable("ServiceUrl");
            _port = port;
        }

        public override void SetUp()
        {
            if (_uri == Const.Uri)
            {
                base.SetUp();
            }
            else
            {
                FIXClient = new FixClient(_targetCompId, _senderCompId, _uri, _port);
                ClientOrderId = Guid.NewGuid().ToString();
                FIXClient.Init();
            }

        }

        [Test]
        public void ShouldPlaceMarketOrder()
        {
            SharedTest.ShouldPlaceMarketOrder(FIXClient, CreateNewOrder(ClientOrderId));

        }

        [Test]
        public void ShouldRejectOnlyOnce()
        {
            SharedTest.ShouldRejectOnlyOnce(FIXClient, CreateNewOrder(ClientOrderId));

        }

        [Test]
        public void ShouldPlaceLimitOrder()
        {
            SharedTest.ShouldPlaceLimitOrder(FIXClient, CreateNewOrder(ClientOrderId, isMarket: false, assetPairId: "BTCLKK", qty: 0.01m, isBuy: false, price: 10591.0m));
        }

        [Test]
        public void ShouldCancelLimitOrder()
        {
            SharedTest.ShouldCancelLimitOrder(FIXClient, CreateNewOrder(ClientOrderId, isMarket: false, assetPairId: "BTCLKK", qty: 0.01m, isBuy: false, price: 5000.0m));
        }

        [Test]
        public void OrderMonkey()
        {
            SharedTest.OrderMonkey(FIXClient, 12000m, "BTCUSD");
        }

        [Test]
        public void ShouldReturnBusinessReject()
        {
            var maintenanceMode = AppSettings.CurrentValue.FixGatewayService.MaintenanceMode;
            maintenanceMode.Enabled = true;
            maintenanceMode.Reason = "SomeReason";

            var request = CreateNewOrder(ClientOrderId);

            FIXClient.Send(request);

            var resp = FIXClient.GetResponse<Message>();

            Assert.That(resp, Is.Not.Null);
            Assert.That(resp, Is.TypeOf<BusinessMessageReject>());
            Assert.That(resp.GetString(Tags.Text), Is.EqualTo(maintenanceMode.Reason));
        }
    }


    internal static class SharedTest
    {
        public static void ShouldPlaceMarketOrder(FixClient fixClient, NewOrderSingle orderRequest)
        {
            fixClient.Send(orderRequest);

            var response = fixClient.GetResponse<Message>();

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<ExecutionReport>());

            var ex = (ExecutionReport)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.PENDING_NEW));
            Assert.That(ex.ExecType.Obj, Is.EqualTo(ExecType.PENDING_NEW));


            response = fixClient.GetResponse<Message>();

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<ExecutionReport>());

            ex = (ExecutionReport)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.FILLED));
            Assert.That(ex.ExecType.Obj, Is.EqualTo(ExecType.TRADE));
            Assert.That(ex.LastQty.Obj, Is.EqualTo(orderRequest.OrderQty.Obj));
            Assert.That(ex.LastPx.Obj, Is.GreaterThan(0));
        }

        public static void ShouldPlaceLimitOrder(FixClient fixClient, NewOrderSingle orderRequest)
        {
            fixClient.Send(orderRequest);

            var response = fixClient.GetResponse<Message>();

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<ExecutionReport>());

            var ex = (ExecutionReport)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.PENDING_NEW));
            Assert.That(ex.ExecType.Obj, Is.EqualTo(ExecType.PENDING_NEW));

        }

        public static void ShouldCancelLimitOrder(FixClient fixClient, NewOrderSingle orderRequest)
        {
            var cleintOrdId = orderRequest.ClOrdID.Obj;
            fixClient.Send(orderRequest);

            var response = fixClient.GetResponse<Message>();

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<ExecutionReport>());

            var ex = (ExecutionReport)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.PENDING_NEW));
            Assert.That(ex.ExecType.Obj, Is.EqualTo(ExecType.PENDING_NEW));

            var cancleRequest = new OrderCancelRequest
            {
                ClOrdID = new ClOrdID(Guid.NewGuid().ToString()),
                OrigClOrdID = new OrigClOrdID(cleintOrdId),
                TransactTime = new TransactTime(DateTime.UtcNow)
            };

            fixClient.Send(cancleRequest);

            response = fixClient.GetResponse<Message>();

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<ExecutionReport>());

            ex = (ExecutionReport)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.PENDING_CANCEL));
            Assert.That(ex.ExecType.Obj, Is.EqualTo(ExecType.PENDING_CANCEL));

            response = fixClient.GetResponse<Message>();

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<ExecutionReport>());
            ex = (ExecutionReport)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.CANCELED));
            Assert.That(ex.ExecType.Obj, Is.EqualTo(ExecType.CANCELED));

        }



        public static void OrderMonkey(FixClient fixClient, decimal basePrice, string assetId)
        {
            var startTime = DateTime.Now;
            var priceRnd = new Random();
            var volumeRnd = new Random();
            while ((DateTime.Now - startTime).TotalSeconds < 60)
            {
                var sellPrice = basePrice + 50 + priceRnd.Next(50);
                var sellLimitOrd = TradeSessionIntegrationBase.CreateNewOrder(Guid.NewGuid().ToString(), false, false, assetId, 0.01m + (decimal)(volumeRnd.NextDouble() * 0.01), sellPrice);

                var buyPrice = basePrice - 50 - priceRnd.Next(50);
                var buyLimitOrd = TradeSessionIntegrationBase.CreateNewOrder(Guid.NewGuid().ToString(), false, true, assetId, 0.01m + (decimal)(volumeRnd.NextDouble() * 0.01), buyPrice);

                fixClient.Send(sellLimitOrd);
                fixClient.GetResponse<Message>();

                fixClient.Send(buyLimitOrd);
                fixClient.GetResponse<Message>();


                var sellMrkOrder = TradeSessionIntegrationBase.CreateNewOrder(Guid.NewGuid().ToString(), true, false, assetId, 0.11m + (decimal)(volumeRnd.NextDouble() * 0.01));
                var buyMrkOrder = TradeSessionIntegrationBase.CreateNewOrder(Guid.NewGuid().ToString(), true, true, assetId, 0.11m + (decimal)(volumeRnd.NextDouble() * 0.01));

                fixClient.Send(sellMrkOrder);
                fixClient.GetResponse<Message>();

                fixClient.Send(buyMrkOrder);
                fixClient.GetResponse<Message>();


            }

            while (fixClient.GetResponse<Message>(1000) != null)
            {
                // Read all messages from the socket
            }

        }

        public static void ShouldRejectOnlyOnce(FixClient fixClient, NewOrderSingle orderRequest)
        {
            orderRequest.OrderQty = new OrderQty(-1);
            fixClient.Send(orderRequest);

            var response = fixClient.GetResponse<Message>(50000);

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<ExecutionReport>());

            var ex = (ExecutionReport)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.REJECTED));
            Assert.That(ex.ExecType.Obj, Is.EqualTo(ExecType.REJECTED));


            response = fixClient.GetResponse<Message>(10000);

            Assert.That(response, Is.Null);

        }
    }
}
