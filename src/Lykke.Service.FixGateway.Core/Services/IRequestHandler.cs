using System;
using System.Threading.Tasks;
using QuickFix;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IRequestHandler<in T> : IDisposable where T : Message
    {
        Task Handle(T request);
    }
}
