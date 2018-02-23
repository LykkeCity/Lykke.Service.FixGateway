using System;
using NUnit.Framework;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests.TradeSessionIntegration
{
    internal abstract class ExternalServiceFailedBase : TradeSessionIntegrationBase
    {

        [Test]
        public void ShouldRejectMarketOrderIfTimeout()
        {
            var orderRequest = CreateNewOrder(ClientOrderId);
            FIXClient.Send(orderRequest);

            var response = FIXClient.GetResponse<Message>();

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<ExecutionReport>());

            var ex = (ExecutionReport)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.REJECTED));
            Assert.That(ex.ExecType.Obj, Is.EqualTo(ExecType.REJECTED));
            Assert.That(ex.OrdRejReason.Obj, Is.EqualTo(OrdRejReason.OTHER));

            response = FIXClient.GetResponse<Message>();

            Assert.That(response, Is.Null);

        }

        [Test]
        public void ShouldRejectOrderCancelRequestIfTimeout()
        {
            var clientOrderID = Guid.NewGuid().ToString();
            var cancleRequest = new OrderCancelRequest
            {
                ClOrdID = new ClOrdID(Guid.NewGuid().ToString()),
                OrigClOrdID = new OrigClOrdID(clientOrderID),
                TransactTime = new TransactTime(DateTime.UtcNow)
            };

            FIXClient.Send(cancleRequest);

            var response = FIXClient.GetResponse<Message>();

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<OrderCancelReject>());

            var ex = (OrderCancelReject)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.REJECTED));
            Assert.That(ex.IsSetCxlRejReason(),Is.True);
            Assert.That(ex.IsSetCxlRejResponseTo,Is.True);

            response = FIXClient.GetResponse<Message>();

            Assert.That(response, Is.Null);

        }

        [Test]
        public void ShouldRejectLimitOrderIfTimeout()
        {
            var orderRequest = CreateNewOrder(ClientOrderId, false, true, price: 10000);
            FIXClient.Send(orderRequest);

            var response = FIXClient.GetResponse<Message>();

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<ExecutionReport>());

            var ex = (ExecutionReport)response;
            Assert.That(ex.OrdStatus.Obj, Is.EqualTo(OrdStatus.REJECTED));
            Assert.That(ex.ExecType.Obj, Is.EqualTo(ExecType.REJECTED));
            Assert.That(ex.OrdRejReason.Obj, Is.EqualTo(OrdRejReason.OTHER));

            response = FIXClient.GetResponse<Message>();

            Assert.That(response, Is.Null);

        }
    }
}
