using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Storage
{
    public interface INamedSemaphore
    {
        Task<IAsyncDisposable> WaitAsync(string handle, TimeSpan timeout, CancellationToken cancellationToken);
    }
}
