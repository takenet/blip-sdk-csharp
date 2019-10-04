using System;

namespace Take.Blip.Builder.Hosting
{
    public interface IConfiguration
    {
        TimeSpan InputProcessingTimeout { get; }

        string RedisStorageConfiguration { get; }

        int RedisDatabase { get; }

        string RedisKeyPrefix { get; }

        int MaxTransitionsByInput { get; }

        int TraceQueueBoundedCapacity { get; }

        int TraceQueueMaxDegreeOfParallelism { get; }

        TimeSpan TraceTimeout { get; }

        bool ContactCacheEnabled { get; }
        
        TimeSpan ContactCacheExpiration { get; }
        
        TimeSpan DefaultActionExecutionTimeout { get; }
    }
}