using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface IDistributedMutex
    {
        /// <summary>
        /// Waits the asynchronous.
        /// </summary>
        /// <param name="handle">The handle name.</param>
        /// <param name="timeout">The timeout to holding the lock.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<IAsyncDisposable> WaitAsync(string handle, TimeSpan timeout, CancellationToken cancellationToken);
    }
}
