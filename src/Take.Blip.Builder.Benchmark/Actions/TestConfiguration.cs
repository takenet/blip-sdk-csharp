using System;
using System.Diagnostics.CodeAnalysis;
using Take.Blip.Builder.Hosting;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Take.Blip.Builder.Benchmark.Actions
{
    /// <inheritdoc />
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class TestConfiguration : IConfiguration
    {
        public TimeSpan InputProcessingTimeout { get; set; }
        public int RedisDatabase { get; set; }
        public string RedisKeyPrefix { get; set; }
        public string InternalUris { get; set; }
        public int MaxTransitionsByInput { get; set; }
        public int TraceQueueBoundedCapacity { get; set; }
        public int TraceQueueMaxDegreeOfParallelism { get; set; }
        public TimeSpan TraceTimeout { get; set; }
        public TimeSpan DefaultActionExecutionTimeout { get; set; }
        public int ExecuteScriptLimitRecursion { get; set; }
        public int ExecuteScriptMaxStatements { get; set; }
        public long ExecuteScriptLimitMemory { get; set; }
        public long ExecuteScriptLimitMemoryWarning { get; set; }
        public TimeSpan ExecuteScriptTimeout { get; set; }
        public TimeSpan ExecuteScriptV2Timeout { get; set; }
        public int MaximumInputExpirationLoop { get; set; }
        public long ExecuteScriptV2MaxRuntimeHeapSize { get; set; }
        public long ExecuteScriptV2MaxRuntimeStackUsage { get; set; }
    }
}