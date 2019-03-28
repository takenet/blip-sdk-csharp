using Lime.Messaging.Resources;
using StackExchange.Redis;
using Take.Blip.Builder.Hosting;
using Take.Elephant;
using Take.Elephant.Redis;

namespace Take.Blip.Builder.Storage.Redis
{
    public class CacheOwnerCallerContactMap : RedisStringMap<OwnerCaller, Contact>, ICacheOwnerCallerContactMap
    {
        public CacheOwnerCallerContactMap(
            ISerializer<Contact> serializer,
            IConfiguration configuration,
            IConnectionMultiplexer connectionMultiplexer)
            : base($"{configuration.RedisKeyPrefix}:builder-contacts-cache", connectionMultiplexer, serializer, configuration.RedisDatabase)
        {
        }
    }
}