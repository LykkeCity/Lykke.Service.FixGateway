using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Services;

namespace Lykke.Service.FixGateway.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<ISupportInit>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.

    [UsedImplicitly]
    public abstract class StartupManager : IStartupManager
    {
        private readonly IEnumerable<ISessionManager> _sessionManagers;

        protected StartupManager(IEnumerable<ISessionManager> sessionManagers)
        {
            _sessionManagers = sessionManagers;
        }

        public virtual Task StartAsync()
        {
            foreach (var manager in _sessionManagers)
            {
                manager.Init();
            }
            return Task.CompletedTask;
        }
    }
}
