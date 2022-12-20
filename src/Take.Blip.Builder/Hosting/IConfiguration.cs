using System;
using System.Threading;

namespace Take.Blip.Builder.Hosting
{
    public interface IConfiguration
    {
        /// <summary>
        /// The input processing timeout during message processing
        /// </summary>
        TimeSpan InputProcessingTimeout { get; }

        /// <summary>
        /// Defines whether Unit of Work will be used
        /// </summary>
        bool UseUnitOfWork { get; }

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