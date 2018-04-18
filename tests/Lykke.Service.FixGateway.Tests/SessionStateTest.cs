using System;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using QuickFix;
using ILog = Common.Log.ILog;
using SessionState = Lykke.Service.FixGateway.Services.SessionState;

namespace Lykke.Service.FixGateway.Tests
{
    [TestFixture]
    public class SessionStateTest
    {
        private SessionState _state;
        private ILog _log;

        [SetUp]
        public void SetUp()
        {
            _log = Substitute.For<ILog>();
            _state = new SessionState(new SessionID("", "", ""), _log);
        }

        [Test]
        public void ShouldWarnIfNoAnswerFromMe()
        {
            _state.RegisterRequest("42", "SomeMessage");
            Thread.Sleep(TimeSpan.FromSeconds(6)); // Default timeout is 5 sec
            _log.ReceivedWithAnyArgs(1).WriteWarningAsync("", "", "", "");
        }
    }
}
