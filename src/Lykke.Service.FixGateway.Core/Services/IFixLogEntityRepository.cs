using System;
using Common;
using Lykke.Service.FixGateway.Services.Logging;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IFixLogEntityRepository : IStopable
    {
        void WriteLogItem(DateTime time, string senderCompId, string targetCompId, string message, FixMessageDirection direction);
    }
}
