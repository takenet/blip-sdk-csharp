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

        /// <summary>
        /// indicates whether the variables previous state id and state id should be merged into a single variable in the database or not
        /// </summary>
        public bool UnifyStateIdAndPreviousStateId { get; }
    }
}