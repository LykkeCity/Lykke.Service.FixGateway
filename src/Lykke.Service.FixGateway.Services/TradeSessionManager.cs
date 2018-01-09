using System;
using System.Collections.Concurrent;
using Autofac;
using Common.Log;
using JetBrains.Annotations;
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
    public class TradeSessionManager : ISessionManager
    {
        private readonly Credentials _credentials;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILog _log;
        private readonly ThreadedSocketAcceptor _socketAcceptor;
        private readonly ConcurrentDictionary<SessionID, ILifetimeScope> _sessionContainers = new ConcurrentDictionary<SessionID, ILifetimeScope>();

        public TradeSessionManager(SessionSetting setting, Credentials credentials, ILifetimeScope lifetimeScope, ILog log)
        {
            _credentials = credentials;
            _lifetimeScope = lifetimeScope;
            _log = log.CreateComponentScope(nameof(TradeSessionManager));

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
            _log.WriteInfoAsync("ToAdmin", "", "");
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            if (message is Logon logon)
            {
                EnsureHasPassword(logon, sessionID);
            }
            _log.WriteInfoAsync("FromAdmin", "", "");
        }

        public void ToApp(Message message, SessionID sessionId)
        {
            _log.WriteInfoAsync("ToApp", "", "");
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            _log.WriteInfoAsync("FromApp", "", "");
            dynamic msg = message;
            HandleRequest(msg, sessionID);

        }

        public void OnCreate(SessionID sessionID)
        {
            _log.WriteInfoAsync("A new session created", $"SessionID: {sessionID}", "");
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
                    _log.WriteWarningAsync("User logout", $"SessionID {sessionID}", "Unexpected exception", e);
                }
            }
            _log.WriteInfoAsync("Session closed", $"SenderCompID: {sessionID.SenderCompID}", "");

        }

        public virtual void OnLogon(SessionID sessionID)
        {
            Init(sessionID);
            _log.WriteInfoAsync("User logged in", $"SenderCompID: {sessionID.SenderCompID}", "").GetAwaiter().GetResult();
        }

        private void HandleRequest(NewOrderSingle request, SessionID sessionID)
        {
            if (_sessionContainers.TryGetValue(sessionID, out var scope))
            {
                scope.Resolve<NewOrderRequestHandler>()
                    .Handle(request);
            }
            else
            {
                _log.WriteWarningAsync("Handle NewOrderSingle", $"SessionID:{sessionID}", "Inconsistent state of the session. Inform developers about this.").GetAwaiter().GetResult();
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void EnsureHasPassword(Logon message, SessionID sessionID)
        {
            if (!string.Equals(_credentials.Password, message.Password.Obj, StringComparison.Ordinal))
            {
                _log.WriteWarningAsync(nameof(EnsureHasPassword), $"SenderCompID: {sessionID.SenderCompID}", "Incorrect password");
                throw new RejectLogon("Incorrect password");
            }
        }

        private void Init(SessionID sessionID)
        {
            try
            {
                var innerScope = _lifetimeScope.BeginLifetimeScope();
                var op = innerScope.Resolve<IClientOrderIdProvider>();
                op.Start();
                innerScope.Resolve<NewOrderRequestHandler>(TypedParameter.From(new SessionState(sessionID)));
                innerScope.Resolve<MatchingEngineNotificationListener>(TypedParameter.From(new SessionState(sessionID)));
                _sessionContainers.TryAdd(sessionID, innerScope);
            }
            catch (Exception ex)
            {
                _log.WriteWarningAsync("New session initialization", $"SessionID: {sessionID}", "Unable initialize a new session", ex);
                throw new RejectLogon("Internal error");
            }

        }
    }

}
