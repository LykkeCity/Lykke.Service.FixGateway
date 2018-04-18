using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Common.Log;
using QuickFix;
using ILog = Common.Log.ILog;

namespace Lykke.Service.FixGateway.Services
{
    public sealed class SessionState : IDisposable
    {
        private readonly ILog _log;
        private int _orderReportId;
        public SessionID SessionID { get; }
        public int OrderReportId => _orderReportId;
        private readonly ConcurrentDictionary<string, RequestInfo> _requestInfos = new ConcurrentDictionary<string, RequestInfo>();
        private readonly Timer _timer;
        private readonly TimeSpan _refreshPeriod = TimeSpan.FromMilliseconds(500);

        public int NextOrderReportId => Interlocked.Increment(ref _orderReportId);

        public SessionState(SessionID sessionID, ILog log)
        {
            _log = log.CreateComponentScope(nameof(SessionState));
            SessionID = sessionID;
            _timer = new Timer(InvalidateRequests, null, _refreshPeriod, _refreshPeriod);
        }

        public void RegisterRequest(string id, string errorMessage)
        {
            var info = new RequestInfo(errorMessage);
            _requestInfos[id] = info;
        }

        public void ConfirmRequest(string id)
        {
            _requestInfos.TryRemove(id, out _);
        }

        private void InvalidateRequests(object _)
        {
            var now = DateTime.UtcNow;
            foreach (var infoKv in _requestInfos)
            {
                var info = infoKv.Value;
                if (now - info.CreationTime > info.ResponseTimeout)
                {
                    _log.WriteWarning(nameof(InvalidateRequests), null, info.Message);
                    _requestInfos.TryRemove(infoKv.Key, out var _);
                }
            }
        }

        private sealed class RequestInfo
        {
            public string Message { get; }
            public DateTime CreationTime { get; }
            public TimeSpan ResponseTimeout { get; }

            public RequestInfo(string message)
            {
                Message = message;
                CreationTime = DateTime.UtcNow;
                ResponseTimeout = Const.MeRequestTimeout;
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
