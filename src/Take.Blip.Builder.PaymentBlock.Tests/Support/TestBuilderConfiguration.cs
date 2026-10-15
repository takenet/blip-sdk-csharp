using System;
using System.Collections.Generic;
using Take.Blip.Builder.Hosting;

namespace Take.Blip.Builder.PaymentBlock.Tests.Support
{
    /// <summary>
    /// <see cref="IConfiguration"/> do motor para os testes: devolve os valores de <see cref="ConventionsConfiguration"/>
    /// (inclusive o <c>ProcessCommandMetadatasToInsert</c>) e só troca o <c>ExecuteScriptV2Timeout</c>, que lá não é
    /// virtual e por isso é reimplementado aqui em vez de sobrescrito.
    /// </summary>
    public sealed class TestBuilderConfiguration : IConfiguration
    {
        private readonly ConventionsConfiguration _conventions = new ConventionsConfiguration();

        /// <summary>Cria a configuração com o tempo máximo de script informado.</summary>
        /// <param name="executeScriptV2Timeout">Tempo máximo de execução de um <c>ExecuteScriptV2</c>.</param>
        public TestBuilderConfiguration(TimeSpan executeScriptV2Timeout)
        {
            if (executeScriptV2Timeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(executeScriptV2Timeout));
            }

            ExecuteScriptV2Timeout = executeScriptV2Timeout;
        }

        /// <summary>Tempo máximo de script padrão do motor (o de <see cref="ConventionsConfiguration"/>).</summary>
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
