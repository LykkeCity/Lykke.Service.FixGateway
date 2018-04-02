using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.FixGateway.Core.Services;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class MtStartupManager : StartupManager
    {
        public MtStartupManager(IEnumerable<ISessionManager> sessionManagers) : base(sessionManagers)
        {

        }

        public override async Task StartAsync()
        {
            await base.StartAsync();
        }
    }
}
