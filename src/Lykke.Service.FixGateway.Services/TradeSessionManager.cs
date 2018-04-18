using System;
using System.Collections.Concurrent;
using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Logging;
using Lykke.Service.FixGateway.Core.Services;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using QuickFix;
using QuickFix.FIX44;
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

        public TradeSessionManager(SessionSetting setting, Credentials credentials, ILifetimeScope lifetimeScope, IFixLogEntityRepository fixLogEntityRepository, ILog log)
        {
            _credentials = credentials;
            _lifetimeScope = lifetimeScope;
            _log = log.CreateComponentScope(nameof(TradeSessionManager));

            var settings = new SessionSettings(setting.GetFixConfigAsReader());
            var storeFactory = new MemoryStoreFactory();
            var eventLogFactory = new LykkeLogFactory(log, false, false);
            var messagesLogFactory = new AzureLogFactory(fixLogEntityRepository);
            var compositeLogFactory = new CompositeLogFactory(new ILogFactory[] { eventLogFactory, messagesLogFactory });
            _socketAcceptor = new ThreadedSocketAcceptor(this, storeFactory, settings, compositeLogFactory);


        }

        public void Init()
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
                EnsureHasPassword(logon, sessionID);
            }
        }

        public void ToApp(Message message, SessionID sessionId)
        {
            // Nothing to do here
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            if (_sessionContainers.TryGetValue(sessionID, out var scope))
            {
                if (!scope.Resolve<IMaintenanceModeManager>().AllowProcessMessages(message))
                {
                    _log.WriteInfo(nameof(FromApp), "", $"Maintenance mode is active. Ignore {message.GetType().Name} request from  {sessionID}");
                    return;
                }
            }
            else
            {
                _log.WriteWarning("Handle NewOrderSingle", $"SessionID:{sessionID}", "Inconsistent state of the session. Inform developers about this.");
            }


            dynamic msg = message;
            HandleRequest(msg, sessionID);

        }

        public void OnCreate(SessionID sessionID)
        {
            _log.WriteInfo("A new session created", $"SessionID: {sessionID}", "");
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

            _log.WriteInfo("Session closed", $"SenderCompID: {sessionID.TargetCompID}", "");

        }

        public virtual void OnLogon(SessionID sessionID)
        {

            Init(sessionID);
            _log.WriteInfo("User logged in", $"SenderCompID: {sessionID.TargetCompID}", "");
        }

        private void HandleRequest(NewOrderSingle request, SessionID sessionID)
        {
            if (_sessionContainers.TryGetValue(sessionID, out var scope))
            {
                scope.Resolve<INewOrderRequestHandler>().Handle(request).GetAwaiter().GetResult();
            }
            else
            {
                _log.WriteWarning("Handle NewOrderSingle", $"SessionID:{sessionID}", "Inconsistent state of the session. Inform developers about this.");
            }
        }

        private void HandleRequest(OrderCancelRequest request, SessionID sessionID)
        {
            if (_sessionContainers.TryGetValue(sessionID, out var scope))
            {
                scope.Resolve<IOrderCancelRequestHandler>().Handle(request).GetAwaiter().GetResult();
            }
            else
            {
                _log.WriteWarning("Handle NewOrderSingle", $"SessionID:{sessionID}", "Inconsistent state of the session. Inform developers about this.");
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void EnsureHasPassword(Logon message, SessionID sessionID)
        {
            if (!string.Equals(_credentials.Password, message.Password.Obj, StringComparison.Ordinal))
            {
                _log.WriteWarning(nameof(EnsureHasPassword), $"SenderCompID: {sessionID.TargetCompID}", "Incorrect password");
                throw new RejectLogon("Incorrect password");
            }
        }

        private void Init(SessionID sessionID)
        {
            try
            {
                var innerScope = _lifetimeScope.BeginLifetimeScope(c => c.RegisterInstance(new SessionState(sessionID, _log)));
                innerScope.Resolve<IClientOrderIdProvider>().Init();
                innerScope.Resolve<MarketOrderNotificationsListener>().Init();
                innerScope.Resolve<LimitOrderNotificationsListener>().Init();
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
