using System;
using QuickFix;

namespace Lykke.Service.FixGateway.Core.Services
{
    public interface IRequestHandler<in T> : IDisposable where T : Message
    {
        void Handle(T request);
    }
}
