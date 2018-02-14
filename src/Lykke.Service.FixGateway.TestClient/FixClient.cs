using System;
using System.Linq;
using System.Threading;
using Autofac;
using Common;
using Common.Log;
using Lykke.Service.FixGateway.Core.Settings.ServiceSettings;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Lykke;
using QuickFix.Transport;
using Message = QuickFix.Message;

namespace Lykke.Service.FixGateway.TestClient
{
    internal class FixClient : IApplication, IStartable, IStopable
    {
        private readonly string _password;
        private readonly SocketInitiator _socketInitiator;
        private SessionID _sessionId;
        private readonly LogToConsole _log;
        private Message _response;

        public FixClient(string serviceUrl, string password, SessionSetting s)
        {
            _password = password;

            _log = new LogToConsole();
            var settings = new SessionSettings(s.GetFixConfigAsReader());

            var sessionID = settings.GetSessions().First();
            settings.Get(sessionID).SetString(SessionSettings.SOCKET_CONNECT_HOST, serviceUrl);
            settings.Get(sessionID).SetString(SessionSettings.TARGETCOMPID, s.TargetCompID);
            settings.Get(sessionID).SetString(SessionSettings.SENDERCOMPID, s.SenderCompID);

            var storeFactory = new MemoryStoreFactory();
            var logFactory = new LykkeLogFactory(_log);
            _socketInitiator = new SocketInitiator(this, storeFactory, settings, logFactory);
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            _log.WriteInfo("FixClient", "ToAdmin", "");
            if (message is Logon logon)
            {
                logon.Password = new Password(_password);
            }
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            _log.WriteInfo("FixClient", "FromAdmin", "");
        }

        public void ToApp(Message message, SessionID sessionId)
        {
            _log.WriteInfo("FixClient", "ToApp", "");
        }

        public void FromApp(Message message, SessionID sessionID)
        {
            _log.WriteInfo("FixClient", "FromApp", "");
            _response = message;
        }

        public void OnCreate(SessionID sessionID)
        {
            _log.WriteInfo("FixClient", "OnCreate", "");
            _sessionId = sessionID;
        }

        public void OnLogout(SessionID sessionID)
        {
            _log.WriteInfo("FixClient", "OnLogout", "");
        }

        public void OnLogon(SessionID sessionID)
        {
            _log.WriteInfo("FixClient", "OnLogon", "");
        }

        public void Send(Message message)
        {
            var header = message.Header;
            header.SetField(new SenderCompID(_sessionId.SenderCompID));
            header.SetField(new TargetCompID(_sessionId.TargetCompID));

            var result = QuickFix.Session.SendToTarget(message);
            if (!result)
            {
                throw new InvalidOperationException("Unable to send request. Unknown error");
            }
        }

        public T GetResponse<T>() where T : Message
        {
            for (var i = 0; i < 1000; i++)
            {
                if (_response != null)
                {
                    var copy = _response;
                    _response = null;
                    return (T)copy;
                }
                Thread.Sleep(20);
            }
            return null;
        }

        public void Start()
        {
            _socketInitiator.Start();
            for (var i = 0; i < 10000; i++)
            {
                if (_socketInitiator.IsLoggedOn)
                {
                    return;
                }
                Thread.Sleep(6);
            }
        }

        public void Dispose()
        {
            _socketInitiator.Dispose();
        }

        public void Stop()
        {
            _socketInitiator.Stop();
            for (var i = 0; i < 10; i++)
            {
                if (!_socketInitiator.IsLoggedOn)
                {
                    return;
                }
                Thread.Sleep(500);
            }
        }
    }
}
