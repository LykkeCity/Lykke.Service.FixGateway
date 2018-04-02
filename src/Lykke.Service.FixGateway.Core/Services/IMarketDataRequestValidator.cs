using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuickFix.FIX44;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IMarketDataRequestValidator
    {
        Task<bool> ValidateAsync(MarketDataRequest request, IEnumerable<string> subscriptions, CancellationToken token);
    }
}
