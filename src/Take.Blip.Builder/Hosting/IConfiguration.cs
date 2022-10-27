using System;
using System.Threading;

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
        TimeSpan InputProcessingSemaphoreTimeout { get; }

        /// <summary>
        /// Flag that activates or not if the timeout will have a different time from the input and the semaphore.
        /// </summary>
        bool LogicOfTimeoutDifferentFromSemaphoreAndInput { get; }

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