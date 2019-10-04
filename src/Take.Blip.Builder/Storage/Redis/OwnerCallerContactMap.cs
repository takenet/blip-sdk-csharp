using Lime.Messaging.Resources;
using StackExchange.Redis;
using Take.Blip.Builder.Hosting;
using Take.Elephant;
using Take.Elephant.Redis;

namespace Take.Blip.Builder.Storage.Redis
{
    public class OwnerCallerContactMap : RedisStringMap<OwnerCaller, Contact>, IOwnerCallerContactMap
    {
        public OwnerCallerContactMap(
            ISerializer<Contact> serializer,
            IConfiguration configuration,
            IConnectionMultiplexer connectionMultiplexer)
            : base($"{configuration.RedisKeyPrefix}:builder-contacts-cache", connectionMultiplexer, serializer, configuration.RedisDatabase)
        {
        }
    }
}