using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Client.TestKit.Fakes
{
    public class MemoryBucketExtension : IBucketExtension
    {
        private readonly ConcurrentDictionary<string, CacheDocument> _cache;

        public MemoryBucketExtension()
        {
            _cache = new ConcurrentDictionary<string, CacheDocument>();
        }

        public Task<T> GetAsync<T>(string id, CancellationToken cancellationToken = new CancellationToken()) where T : Document
        {
            if (_cache.TryGetValue(id, out var cacheDocument)
                && (cacheDocument.Expiration == null || cacheDocument.Expiration >= DateTimeOffset.UtcNow))
            {
                return Task.FromResult((T)cacheDocument.Document);
            }

            return Task.FromResult(default(T));
        }

        public Task<DocumentCollection> GetIdsAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = new CancellationToken())
        {
            var ids = _cache
                .Where(d => d.Value.Expiration == null || d.Value.Expiration >= DateTimeOffset.UtcNow)
                .Select(pair => pair.Key)                
                .Skip(skip)
                .Take(take)
                .ToArray();

            var result = new DocumentCollection
            {
                ItemType = PlainText.MediaType,
                Items = ids
                    .Select(id => new PlainText { Text = id.ToString() })
                    .ToArray(),
                Total = ids.Length
            };

            return Task.FromResult(result);
        }

        public Task SetAsync<T>(string id, T document, TimeSpan expiration = new TimeSpan(), CancellationToken cancellationToken = new CancellationToken()) where T : Document
        {
            _cache[id] = new CacheDocument(document, DateTimeOffset.UtcNow.Add(expiration));
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id, CancellationToken cancellationToken = new CancellationToken())
        {
            _cache.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        private class CacheDocument
        {
            public CacheDocument(Document document, DateTimeOffset? expiration)
            {
                Document = document;
                Expiration = expiration;
            }

            public Document Document { get; }

            public DateTimeOffset? Expiration { get; }
        }
    }
}
