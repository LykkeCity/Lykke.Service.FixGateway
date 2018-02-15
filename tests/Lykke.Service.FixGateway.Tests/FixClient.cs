using System;
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

namespace Lykke.Service.FixGateway.Tests
{
    internal class FixClient : IApplication, IStartable, IStopable
    {
        private readonly SocketInitiator _socketInitiator;
        private SessionID _sessionId;
        private readonly LogToConsole _log;
        private Message _response;

        public FixClient(string targetCompId = Const.TargetCompId, string senderCompId = Const.SenderCompId, string uri = Const.Uri, int port = Const.Port)
        {
            var s = new SessionSetting
            {
                TargetCompID = targetCompId,
                SenderCompID = senderCompId,
                FixConfiguration = new[]
                {
                    "[DEFAULT]",
                    "ResetOnLogon=Y",
                    "FileStorePath=client",
                    "ConnectionType=initiator",
                    "ReconnectInterval=60",
                    "BeginString=FIX.4.4",
                    @"DataDictionary=ClientFIX44.xml",
                    "SSLEnable=N",
                    @"SSLProtocols=Tls",
                    "SSLValidateCertificates=N",
                    $"SocketConnectPort={port}",
                    "StartTime=00:00:00",
                    "EndTime=00:00:00",
                    "HeartBtInt=10",
                    "LogonTimeout=120",
                    $"SocketConnectHost={uri}",
                    "[SESSION]",
                }
            };
            _log = new LogToConsole();
            var settings = new SessionSettings(s.GetFixConfigAsReader());
            var storeFactory = new MemoryStoreFactory();
            var logFactory = new LykkeLogFactory(_log, false, false, false);
            _socketInitiator = new SocketInitiator(this, storeFactory, settings, logFactory);
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            _log.WriteInfo("FixClient", "ToAdmin", "");
            if (message is Logon logon)
            {
                logon.Password = new Password(Const.Password);
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
            for (var i = 0; i < 1000; i++)
            {
                if (_socketInitiator.IsLoggedOn)
                {
                    return;
                }
                Thread.Sleep(5);
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
