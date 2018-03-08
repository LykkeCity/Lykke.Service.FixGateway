using System;
using System.Threading;
using Lykke.AzureStorage.Tables;
using Lykke.Service.FixGateway.Services.Logging;

namespace Lykke.Service.FixGateway.AzureRepositories
{
    public sealed class FixLogEntity : AzureTableEntity
    {
        public DateTime Time { get; set; }
        public string SenderCompId { get; set; }
        public string TargetCompId { get; set; }
        public string Message { get; set; }
        public FixMessageDirection Direction { get; set; }
        private static int _msgCounter;

        public FixLogEntity()
        {

        }

        public FixLogEntity(DateTime time, string senderCompId, string targetCompId, string message, FixMessageDirection direction)
        {
            Time = time;
            SenderCompId = senderCompId;
            TargetCompId = targetCompId;
            Message = message;
            Direction = direction;

            PartitionKey = targetCompId; // Always use Client's SenderId
            RowKey = time.ToString("s") + Interlocked.Increment(ref _msgCounter);
        }
    }
}
