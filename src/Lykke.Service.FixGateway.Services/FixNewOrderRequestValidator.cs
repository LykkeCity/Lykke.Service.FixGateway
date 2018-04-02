using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using Lykke.Service.FixGateway.Services.Extensions;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class FixNewOrderRequestValidator : IFixNewOrderRequestValidator, IDisposable
    {
        private readonly IFixMessagesSender _messagesSender;
        private readonly IClientOrderIdProvider _clientOrderIdProvider;
        private readonly IAssetsService _assetsService;
        private readonly CancellationTokenSource _tokenSource;
        private readonly Credentials _credentials;

        public FixNewOrderRequestValidator(IClientOrderIdProvider clientOrderIdProvider, IAssetsService assetsService, Credentials credentials, IFixMessagesSender messagesSender)
        {
            _clientOrderIdProvider = clientOrderIdProvider;
            _assetsService = assetsService;
            _credentials = credentials;
            _messagesSender = messagesSender;
            _tokenSource = new CancellationTokenSource();
        }

        public async Task<bool> ValidateAsync(NewOrderSingle request)
        {
            var clOrdId = request.ClOrdID.Obj;
            var orderExists = await _clientOrderIdProvider.CheckExistsAsync(clOrdId);
            var reject = -1;
            if (orderExists)
            {
                reject = OrdRejReason.DUPLICATE_ORDER;
            }

            var allSymbols = (await _assetsService.GetAllAssetPairsAsync(_tokenSource.Token)).Select(a => a.Id).ToHashSet();

            if (!allSymbols.Contains(request.Symbol.Obj))
            {
                reject = OrdRejReason.UNKNOWN_SYMBOL;
            }
            else if (!Guid.TryParse(request.Account.Obj, out var accId) || accId != _credentials.ClientId)
            {
                reject = OrdRejReason.UNKNOWN_ACCOUNT;
            }
            else if (request.OrderQty.Obj <= 0
                     || !new[] { Side.BUY, Side.SELL }.Contains(request.Side.Obj)
                     || !new[] { OrdType.MARKET, OrdType.LIMIT }.Contains(request.OrdType.Obj)
                     || request.OrdType.Obj == OrdType.LIMIT && (!request.IsSetPrice() || request.Price.Obj <= 0m)
                     || !new[] { TimeInForce.GOOD_TILL_CANCEL, TimeInForce.FILL_OR_KILL }.Contains(request.TimeInForce.Obj)
                     || request.OrdType.Obj == OrdType.LIMIT && request.TimeInForce.Obj != TimeInForce.GOOD_TILL_CANCEL)
            {
                reject = OrdRejReason.UNSUPPORTED_ORDER_CHARACTERISTIC;
            }

            if (reject == -1)
            {
                return true;
            }

            var rejectMessage = new ExecutionReport().CreateReject(Guid.NewGuid(), request, "n/a", reject);
            _messagesSender.Send(rejectMessage);
            return false;

        }

        public void Dispose()
        {
            _tokenSource?.Dispose();
        }
    }
}
