using System;
using System.Collections;
using NUnit.Framework;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests.TradeSessionIntegration
{
    [TestFixture]
    internal class TradeSessionLocalIntegrationTest : TradeSessionIntegrationBase
    {

        [Test]
        public void ShouldPlaceMarketOrder()
        {
            SharedTest.ShouldPlaceMarketOrder(FIXClient, CreateNewOrder());

        }

        [Test]
        public void ShouldRejectOnlyOnce()
        {
            SharedTest.ShouldRejectOnlyOnce(FIXClient, CreateNewOrder());

        }
    }

    [TestFixture]
    internal class TradeSessionExternalIntegrationTest : TradeSessionIntegrationBase
    {
        private readonly string _uri = Environment.GetEnvironmentVariable("ServiceUrl");


        public override void SetUp()
        {
            ClientOrderId = Guid.NewGuid().ToString("D");
            FIXClient = new FixClient("LYKKE_T", "SENDER_T", _uri, 12357);
            FIXClient.Start();
        }

        public override void TearDown()
        {
            FIXClient.Stop();
        }

        [Test]
        public void ShouldPlaceMarketOrder()
        {
            SharedTest.ShouldPlaceMarketOrder(FIXClient, CreateNewOrder());

        }

        [Test]
        public void ShouldRejectOnlyOnce()
        {
            SharedTest.ShouldRejectOnlyOnce(FIXClient, CreateNewOrder());

        }
    }


    internal class SharedTest
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

        public static void ShouldRejectOnlyOnce(FixClient fixClient, NewOrderSingle orderRequest)
        {
            orderRequest.OrderQty = new OrderQty(-1);
            fixClient.Send(orderRequest);

            var response = fixClient.GetResponse<Message>();

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<ExecutionReport>());

            var ex = (ExecutionReport)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.REJECTED));
            Assert.That(ex.ExecType.Obj, Is.EqualTo(ExecType.REJECTED));


            response = fixClient.GetResponse<Message>();

            Assert.That(response, Is.Null);

        }
    }
}
