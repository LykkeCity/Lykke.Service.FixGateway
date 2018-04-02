using System;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Services.Extensions
{
    public static class FixMessagesExt
    {
        public static ExecutionReport CreateReject(this ExecutionReport report, Guid newOrderId, NewOrderSingle request, string excecId, int rejectReason, string rejectDescription = null)
        {

            report.OrderID = new OrderID(newOrderId.ToString());
            report.ClOrdID = new ClOrdID(request.ClOrdID.Obj);
            report.ExecID = new ExecID(excecId);
            report.OrdStatus = new OrdStatus(OrdStatus.REJECTED);
            report.ExecType = new ExecType(ExecType.REJECTED);
            report.Symbol = new Symbol(request.Symbol.Obj);
            report.OrderQty = new OrderQty(request.OrderQty.Obj);
            report.OrdType = new OrdType(request.OrdType.Obj);
            report.TimeInForce = new TimeInForce(request.TimeInForce.Obj);
            report.LeavesQty = new LeavesQty(0);
            report.CumQty = new CumQty(0);
            report.AvgPx = new AvgPx(0);
            report.Side = new Side(request.Side.Obj);
            report.OrdRejReason = new OrdRejReason(rejectReason);

            if (rejectDescription != null)
            {
                report.Text = new Text(rejectDescription);
            }
            return report;
        }

        public static Reject CreateReject(this Reject reject, Message request, Guid code)
        {
            reject.RefSeqNum = new RefSeqNum(request.Header.GetInt(Tags.MsgSeqNum));
            reject.RefMsgType = new RefMsgType(request.Header.GetString(Tags.MsgType));
            reject.SessionRejectReason = new SessionRejectReason(SessionRejectReason.OTHER);
            reject.Text = new Text($"Internal error. Code {code}");
            return reject;
        }

        public static MarketDataRequestReject CreateReject(this MarketDataRequestReject request, MarketDataRequest message, MDReqRejReason rejectReason)
        {
            request.MDReqID = message.MDReqID;
            request.MDReqRejReason = rejectReason;
            return request;
        }

        public static OrderCancelReject CreateReject(this OrderCancelReject reject, OrderCancelRequest request, CxlRejReason rejectReason, string rejectDescription = null)
        {
            var ordId = rejectReason.Obj == CxlRejReason.UNKNOWN_ORDER ? "NONE" : Guid.NewGuid().ToString();

            reject.OrderID = new OrderID(ordId);
            reject.OrigClOrdID = request.OrigClOrdID;
            reject.ClOrdID = request.ClOrdID;
            reject.OrdStatus = new OrdStatus(OrdStatus.REJECTED);
            reject.CxlRejReason = rejectReason;
            reject.CxlRejResponseTo = new CxlRejResponseTo(CxlRejResponseTo.ORDER_CANCEL_REQUEST);

            if (rejectDescription != null)
            {
                reject.Text = new Text(rejectDescription);
            }
            return reject;
        }

        public static SecurityList CreateReject(this SecurityList securityList, SecurityListRequest request, SecurityRequestResult reject)
        {
            var id = request.SecurityReqID.Obj;
            securityList.SecurityReqID = request.SecurityReqID;
            securityList.SecurityResponseID = new SecurityResponseID(id + "-Resp");
            securityList.TotNoRelatedSym = new TotNoRelatedSym(0);
            securityList.SecurityRequestResult = reject;
            return securityList;
        }
    }
}
