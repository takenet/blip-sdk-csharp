using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using Take.Blip.Builder.Hosting;

namespace Take.Blip.Builder
{
    public sealed class RedisNamedSemaphore : INamedSemaphore
    {
        public const string KEY_PREFIX = "mutex";

        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public RedisNamedSemaphore(IConfiguration configuration, IConnectionMultiplexer connectionMultiplexer)
        {
            _configuration = configuration;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<IAsyncDisposable> WaitAsync(string handle, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var db = _connectionMultiplexer.GetDatabase(_configuration.RedisDatabase);
            var key = $"{KEY_PREFIX}:{handle}";
            var value = Guid.NewGuid().ToString();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (await db.LockTakeAsync(key, value, timeout))
                {
                    return new DistributedMutexDisposable(db, key, value);
                }

                await Task.Delay(100, cancellationToken);
            }

            throw new InvalidOperationException($"Could not acquire the mutex {key} with value {value}");
        }

        private class DistributedMutexDisposable : IAsyncDisposable
        {
            private readonly IDatabase _database;
            private readonly string _key;
            private readonly string _handle;

            public DistributedMutexDisposable(IDatabase database, string key, string handle)
            {
                _database = database;
                _key = key;
                _handle = handle;
            }

            public void Dispose()
            {
                _database.LockRelease(_key, _handle);
            }

            public Task DisposeAsync()
            {
                return _database.LockReleaseAsync(_key, _handle);
            }
        }
    }
}