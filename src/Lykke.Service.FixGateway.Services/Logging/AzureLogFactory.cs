using Lykke.Service.FixGateway.Core.Services;
using QuickFix;

namespace Lykke.Service.FixGateway.Services.Logging
{
    public sealed class AzureLogFactory : ILogFactory
    {
        private readonly IFixLogEntityRepository _fixLogEntity;

        public AzureLogFactory(IFixLogEntityRepository fixLogEntity)
        {
            _fixLogEntity = fixLogEntity;
        }
        public ILog Create(SessionID sessionID)
        {
            return new AzureFixLog(_fixLogEntity, sessionID);
        }
    }
}
