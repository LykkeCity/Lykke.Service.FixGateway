using NUnit.Framework;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Tests.TradeSessionIntegration
{
    internal abstract class ExternalServiceFailedBase  : TradeSessionIntegrationBase
    {
        
        [Test]
        public void ShouldRejectIfTimeout()
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
    }
}
