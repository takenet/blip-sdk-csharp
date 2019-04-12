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

        int TraceQueueMaxDegreeOfParalelism { get; }

        TimeSpan TraceTimeout { get; }

        TimeSpan OnDemandCacheExpiration { get; }

        string SqlStorageConnectionString { get; }

        string SqlStorageDriverTypeName { get; }

        string ContextType { get; }

        TimeSpan ContactCacheExpiration { get; }
        
        TimeSpan DefaultActionExecutionTimeout { get; }
    }
}