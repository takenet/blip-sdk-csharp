using System;

namespace Take.Blip.Builder.Hosting
{
    public interface IConfiguration
    {
        TimeSpan InputProcessingTimeout { get; }

        string RedisStorageConfiguration { get; }

        int RedisDatabase { get; }

        int MaxTransitionsByInput { get; }
    }
}