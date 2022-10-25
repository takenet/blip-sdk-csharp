using System;

namespace Take.Blip.Builder.Hosting
{
    public interface IConfiguration
    {
        /// <summary>
        /// The input processing timeout after semaphore.
        /// </summary>
        TimeSpan InputProcessingTimeout { get; }

        /// <summary>
        /// The semaphore processing timeout.
        /// </summary>
        TimeSpan SemaphoreProcessingTimeout { get; }

        int RedisDatabase { get; }

        string RedisKeyPrefix { get; }

        int MaxTransitionsByInput { get; }

        int TraceQueueBoundedCapacity { get; }

        int TraceQueueMaxDegreeOfParallelism { get; }

        TimeSpan TraceTimeout { get; }
        
        TimeSpan DefaultActionExecutionTimeout { get; }

        int ExecuteScriptLimitRecursion { get; }

        int ExecuteScriptMaxStatements { get; }

        long ExecuteScriptLimitMemory { get; }

        long ExecuteScriptLimitMemoryWarning { get; }

        TimeSpan ExecuteScriptTimeout { get; }
    }
}