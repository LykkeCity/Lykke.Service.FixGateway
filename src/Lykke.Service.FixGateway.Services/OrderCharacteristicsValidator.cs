using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.Assets.Client;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Services
{
    internal sealed class OrderCharacteristicsValidator : IDisposable
    {
        private readonly IClientOrderIdProvider _clientOrderIdProvider;
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;
        private readonly CancellationTokenSource _tokenSource;
        private readonly Credentials _credentials;

        public OrderCharacteristicsValidator(IClientOrderIdProvider clientOrderIdProvider, IAssetsServiceWithCache assetsServiceWithCache, Credentials credentials)
        {
            _clientOrderIdProvider = clientOrderIdProvider;
            _assetsServiceWithCache = assetsServiceWithCache;
            _credentials = credentials;
            _tokenSource = new CancellationTokenSource();
        }

        public async Task<OrdRejReason> ValidateRequestAsync(NewOrderSingle request)
        {
            var clOrdId = request.ClOrdID.Obj;
            var orderExists = await _clientOrderIdProvider.CheckExistsAsync(clOrdId);
            var reject = -1;
            if (orderExists)
            {
                reject = OrdRejReason.DUPLICATE_ORDER;
            }

            var allSymbols = (await _assetsServiceWithCache.GetAllAssetPairsAsync(_tokenSource.Token)).Select(a => a.Id).ToHashSet();

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

            if (reject != -1)
            {
                return new OrdRejReason(reject);

            }

            return null;
        }

        public void Dispose()
        {
            _tokenSource.Dispose();
        }
    }
}
