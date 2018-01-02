using System;

namespace Take.Blip.Builder.Hosting
{
    public interface IConfiguration
    {
        TimeSpan ExecutionSemaphoreExpiration { get; }

        string RedisStorageConfiguration { get; }

        int RedisDatabase { get; }
    }
}