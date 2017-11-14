using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface INamedLock
    {
        Task<IDisposable> AcquireAsync(string name, CancellationToken cancellationToken);
    }
}