using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public sealed class MemoryNamedSemaphore : INamedSemaphore
    {
        private readonly ConcurrentDictionary<string, AsyncDisposableSemaphore> _handleSemaphoreDictionary;

        public MemoryNamedSemaphore()
        {
            _handleSemaphoreDictionary = new ConcurrentDictionary<string, AsyncDisposableSemaphore>();
        }

        public async Task<IAsyncDisposable> WaitAsync(string handle, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var semaphore = _handleSemaphoreDictionary.GetOrAdd(handle.ToLowerInvariant(), k => new AsyncDisposableSemaphore());
            await semaphore.SemaphoreSlim.WaitAsync(timeout, cancellationToken);
            return semaphore;
        }

        class AsyncDisposableSemaphore : IAsyncDisposable
        {
            internal SemaphoreSlim SemaphoreSlim { get; }

            public AsyncDisposableSemaphore()
            {
                SemaphoreSlim = new SemaphoreSlim(1, 1);
            }

            public void Dispose()
            {
                SemaphoreSlim.Release();
            }

            public Task DisposeAsync()
            {
                SemaphoreSlim.Release();
                return Task.CompletedTask;
            }
        }
    }
}