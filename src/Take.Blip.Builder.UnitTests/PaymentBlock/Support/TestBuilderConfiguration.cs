using System;
using System.Collections.Generic;
using Take.Blip.Builder.Hosting;

namespace Take.Blip.Builder.UnitTests.PaymentBlock.Support
{
    /// <summary>
    /// Engine <see cref="IConfiguration"/> for the tests: returns the values of <see cref="ConventionsConfiguration"/>
    /// (including <c>ProcessCommandMetadatasToInsert</c>) and only replaces <c>ExecuteScriptV2Timeout</c>, which is not
    /// virtual there and is therefore reimplemented here instead of overridden.
    /// </summary>
    public sealed class TestBuilderConfiguration : IConfiguration
    {
        private readonly ConventionsConfiguration _conventions = new ConventionsConfiguration();

        /// <summary>Creates the configuration with the given maximum script time.</summary>
        /// <param name="executeScriptV2Timeout">Maximum execution time of an <c>ExecuteScriptV2</c>.</param>
        public TestBuilderConfiguration(TimeSpan executeScriptV2Timeout)
        {
            if (executeScriptV2Timeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(executeScriptV2Timeout));
            }

            ExecuteScriptV2Timeout = executeScriptV2Timeout;
        }

        /// <summary>Engine default maximum script time (the one from <see cref="ConventionsConfiguration"/>).</summary>
        public static TimeSpan DefaultExecuteScriptV2Timeout => new ConventionsConfiguration().ExecuteScriptV2Timeout;

        /// <inheritdoc />
        public TimeSpan InputProcessingTimeout => _conventions.InputProcessingTimeout;

        /// <inheritdoc />
        public int RedisDatabase => _conventions.RedisDatabase;

        /// <inheritdoc />
        public string RedisKeyPrefix => _conventions.RedisKeyPrefix;

        /// <inheritdoc />
        public string InternalUris => _conventions.InternalUris;

        /// <inheritdoc />
        public int MaxTransitionsByInput => _conventions.MaxTransitionsByInput;

        /// <inheritdoc />
        public int TraceQueueBoundedCapacity => _conventions.TraceQueueBoundedCapacity;

        /// <inheritdoc />
        public int TraceQueueMaxDegreeOfParallelism => _conventions.TraceQueueMaxDegreeOfParallelism;

        /// <inheritdoc />
        public TimeSpan TraceTimeout => _conventions.TraceTimeout;

        /// <inheritdoc />
        public TimeSpan DefaultActionExecutionTimeout => _conventions.DefaultActionExecutionTimeout;

        /// <inheritdoc />
        public int ExecuteScriptLimitRecursion => _conventions.ExecuteScriptLimitRecursion;

        /// <inheritdoc />
        public int ExecuteScriptMaxStatements => _conventions.ExecuteScriptMaxStatements;

        /// <inheritdoc />
        public long ExecuteScriptLimitMemory => _conventions.ExecuteScriptLimitMemory;

        /// <inheritdoc />
        public long ExecuteScriptLimitMemoryWarning => _conventions.ExecuteScriptLimitMemoryWarning;

        /// <inheritdoc />
        public TimeSpan ExecuteScriptTimeout => _conventions.ExecuteScriptTimeout;

        /// <inheritdoc />
        public TimeSpan ExecuteScriptV2Timeout { get; }

        /// <inheritdoc />
        public int MaximumInputExpirationLoop => _conventions.MaximumInputExpirationLoop;

        /// <inheritdoc />
        public long ExecuteScriptV2MaxRuntimeHeapSize => _conventions.ExecuteScriptV2MaxRuntimeHeapSize;

        /// <inheritdoc />
        public long ExecuteScriptV2MaxRuntimeStackUsage => _conventions.ExecuteScriptV2MaxRuntimeStackUsage;

        /// <inheritdoc />
        public Dictionary<string, string> ProcessCommandMetadatasToInsert => _conventions.ProcessCommandMetadatasToInsert;
    }
}
