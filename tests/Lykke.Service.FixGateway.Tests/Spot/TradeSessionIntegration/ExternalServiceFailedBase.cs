using System;
using NUnit.Framework;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests.Spot.TradeSessionIntegration
{
    internal abstract class ExternalServiceFailedBase : TradeSessionIntegrationBase
    {

        [Test]
        public void ShouldRejectMarketOrderIfTimeout()
        {
            FIXClient.GetAdminResponse<Message>();// Ignore logon
            var orderRequest = CreateNewOrder(ClientOrderId);
            FIXClient.Send(orderRequest);

            var response = FIXClient.GetAdminResponse<Reject>(10000);

            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<Reject>());
            Assert.That(response.SessionRejectReason.Obj, Is.EqualTo(SessionRejectReason.OTHER));

        }

//        [Test]
//        public void ShouldRejectOrderCancelRequestIfTimeout()
//        {
//            var clientOrderID = Guid.NewGuid().ToString();
//            var cancleRequest = new OrderCancelRequest
//            {
//                ClOrdID = new ClOrdID(Guid.NewGuid().ToString()),
//                OrigClOrdID = new OrigClOrdID(clientOrderID),
//                TransactTime = new TransactTime(DateTime.UtcNow)
//            };
//
//            FIXClient.GetAdminResponse<Message>();// Ignore logon
//
//            FIXClient.Send(cancleRequest);
//
//            var response = FIXClient.GetAdminResponse<Reject>(10000);
//
//            Assert.That(response, Is.Not.Null);
//            Assert.That(response, Is.TypeOf<Reject>());
//            Assert.That(response.SessionRejectReason, Is.EqualTo(SessionRejectReason.OTHER));
//
//        }

        [Test]
        public void ShouldRejectLimitOrderIfTimeout()
        {

            FIXClient.GetAdminResponse<Message>();// Ignore logon

            var orderRequest = CreateNewOrder(ClientOrderId, false, true, price: 10000);

            FIXClient.Send(orderRequest);

            var response = FIXClient.GetAdminResponse<Reject>(10000);


            Assert.That(response, Is.Not.Null);
            Assert.That(response, Is.TypeOf<Reject>());
            Assert.That(response.SessionRejectReason.Obj, Is.EqualTo(SessionRejectReason.OTHER));

        }
    }
}
