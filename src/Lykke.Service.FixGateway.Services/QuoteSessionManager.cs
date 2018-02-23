using System;
using System.Collections.Concurrent;
using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using QuickFix;
using QuickFix.FIX44;
using QuickFix.Lykke;
using ILog = Common.Log.ILog;
using Message = QuickFix.Message;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class QuoteSessionManager : ISessionManager
    {
        private readonly Credentials _credentials;
        private readonly ILifetimeScope _lifetimeScope;

        private readonly ILog _log;
        private readonly ThreadedSocketAcceptor _socketAcceptor;

        private readonly ConcurrentDictionary<SessionID, ILifetimeScope> _sessionContainers = new ConcurrentDictionary<SessionID, ILifetimeScope>();


        public QuoteSessionManager(SessionSetting setting, Credentials credentials, IAssetsServiceWithCache assetsService, ILifetimeScope lifetimeScope, ILog log)
        {
            _lifetimeScope = lifetimeScope;
            _credentials = credentials;
            _log = log.CreateComponentScope(nameof(QuoteSessionManager));

            var settings = new SessionSettings(setting.GetFixConfigAsReader());
            var storeFactory = new MemoryStoreFactory();
            var logFactory = new LykkeLogFactory(log, false, false);
            _socketAcceptor = new ThreadedSocketAcceptor(this, storeFactory, settings, logFactory);


        }

        public void Start()
        {
            _socketAcceptor.Start();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (var lifetimeScope in _sessionContainers)
            {
                lifetimeScope.Value.Dispose();
            }
            Stop();
            _socketAcceptor.Dispose();
        }


        public void Stop()
        {
            _socketAcceptor.Stop();
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            // Nothing to do here
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            if (message is Logon logon)
            {
                EnsureHasPassword(logon);
            }
        }

        public void ToApp(Message message, SessionID sessionId)
        {
            // Nothing to do here
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            dynamic msg = message;
            HandleRequest(msg, sessionID);

        }

        public void OnCreate(SessionID sessionID)
        {
            // Nothing to do here
        }

        public virtual void OnLogout(SessionID sessionID)
        {
            if (_sessionContainers.TryGetValue(sessionID, out var innerScope))
            {
                try
                {
                    innerScope.Dispose();
                    _sessionContainers.TryRemove(sessionID, out _);
                }
                catch (Exception e)
                {
                    _log.WriteWarning("User logout", $"SessionID {sessionID}", "Unexpected exception", e);
                }
            }

            _log.WriteInfo("Session closed", $"SenderCompID: {sessionID.SenderCompID}", "");

        }

        public virtual void OnLogon(SessionID sessionID)
        {
            Init(sessionID);
            _log.WriteInfo("User logged in", $"SenderCompID: {sessionID.SenderCompID}", "");
        }

        private void HandleRequest(SecurityListRequest request, SessionID sessionID)
        {
            if (_sessionContainers.TryGetValue(sessionID, out var scope))
            {
                scope.Resolve<AssetsListRequestHandler>().Handle(request);
            }
            else
            {
                _log.WriteWarning("Handle SecurityListRequest", $"SessionID:{sessionID}", "Inconsistent state of the session. Inform developers about this.");
            }
        }

        private void HandleRequest(MarketDataRequest request, SessionID sessionID)
        {
            if (_sessionContainers.TryGetValue(sessionID, out var scope))
            {
                scope.Resolve<MarketDataRequestHandler>().Handle(request);
            }
            else
            {
                _log.WriteWarning("Handle MarketDataRequest", $"SessionID:{sessionID}", "Inconsistent state of the session. Inform developers about this.");
            }
        }


        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void EnsureHasPassword(Logon message)
        {
            if (!string.Equals(_credentials.Password, message.Password.Obj, StringComparison.Ordinal))
            {
                throw new RejectLogon("Incorrect password");
            }
        }

        private void Init(SessionID sessionID)
        {
            try
            {
                var innerScope = _lifetimeScope.BeginLifetimeScope();
                var sessionState = new SessionState(sessionID);
                innerScope.Resolve<AssetsListRequestHandler>(TypedParameter.From(sessionState));
                innerScope.Resolve<MarketDataRequestHandler>(TypedParameter.From(sessionState));
                _sessionContainers.TryAdd(sessionID, innerScope);
            }
            catch (Exception ex)
            {
                _log.WriteWarning("New session initialization", $"SessionID: {sessionID}", "Unable initialize a new session", ex);
                throw new RejectLogon("Internal error");
            }

        }

    }
}
