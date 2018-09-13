using StackExchange.Redis;
using Take.Blip.Builder.Hosting;
using Take.Elephant;
using Take.Elephant.Redis;

namespace Take.Blip.Builder.Storage.Redis
{
    public class OwnerCallerNameDocumentMap : RedisStringMap<OwnerCallerName, StorageDocument>, ICacheOwnerCallerNameDocumentMap
    {
        public OwnerCallerNameDocumentMap(
            ISerializer<StorageDocument> serializer,
            IConfiguration configuration,
            IConnectionMultiplexer connectionMultiplexer)
            : base($"{configuration.RedisKeyPrefix}:context-documents", connectionMultiplexer, serializer, configuration.RedisDatabase)
        {

        }
    }
}
