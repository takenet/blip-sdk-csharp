using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using NSubstitute;
using Serilog;
using Take.Blip.Builder.Actions.ExecuteScript;
using Take.Blip.Builder.Actions.ExecuteScriptV2;
using Take.Blip.Builder.Benchmark.Context;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Benchmark.Actions
{
    /// <inheritdoc />
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class ExecuteScriptBenchmarkTests : ActionTestsBase
    {
        private ExecuteScriptV2Action _v2Action;
        private ExecuteScriptAction _v1Action;

        /// <summary>
        /// Setup the benchmark tests.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            var configuration = new TestConfiguration();
            var conventions = new ConventionsConfiguration();

            configuration.ExecuteScriptV2Timeout = TimeSpan.FromMilliseconds(300);
            configuration.ExecuteScriptV2MaxRuntimeHeapSize =
                conventions.ExecuteScriptV2MaxRuntimeHeapSize;
            configuration.ExecuteScriptV2MaxRuntimeStackUsage =
                conventions.ExecuteScriptV2MaxRuntimeStackUsage;

            configuration.ExecuteScriptTimeout = TimeSpan.FromMilliseconds(300);
            configuration.ExecuteScriptLimitMemory = conventions.ExecuteScriptLimitMemory;
            configuration.ExecuteScriptLimitRecursion = 100000;
            configuration.ExecuteScriptMaxStatements = 0;

            _v2Action = new ExecuteScriptV2Action(configuration, Substitute.For<IHttpClient>(),
                Substitute.For<ILogger>());

            _v1Action = new ExecuteScriptAction(configuration, Substitute.For<ILogger>());
        }

        /// <summary>
        /// Execute a loop script using the V1 action.
        /// </summary>
        [Benchmark]
        public async Task ExecuteScriptV1LoopScript()
        {
            await _v1Action.ExecuteAsync(Context, Settings._v1LoopSettings, CancellationToken);
        }

        /// <summary>
        /// Execute a loop script using the V2 action.
        /// </summary>
        [Benchmark]
        public async Task ExecuteScriptV2LoopScript()
        {
            await _v2Action.ExecuteAsync(Context, Settings._v2LoopSettings, CancellationToken);
        }

        /// <summary>
        /// Execute a math script using the V1 action.
        /// </summary>
        [Benchmark]
        public async Task ExecuteScriptV1MathScript()
        {
            await _v1Action.ExecuteAsync(Context, Settings._v1MathSettings, CancellationToken);
        }

        /// <summary>
        /// Execute a math script using the V2 action.
        /// </summary>
        [Benchmark]
        public async Task ExecuteScriptV2MathScript()
        {
            await _v2Action.ExecuteAsync(Context, Settings._v2MathSettings, CancellationToken);
        }

        /// <summary>
        /// Execute a json script using the V1 action.
        /// </summary>
        [Benchmark]
        public async Task ExecuteScriptV1JsonScript()
        {
            await _v1Action.ExecuteAsync(Context, Settings._v1JsonSettings, CancellationToken);
        }

        /// <summary>
        /// Execute a json script using the V2 action.
        /// </summary>
        [Benchmark]
        public async Task ExecuteScriptV2JsonScript()
        {
            await _v2Action.ExecuteAsync(Context, Settings._v2JsonSettings, CancellationToken);
        }

        /// <summary>
        /// Execute a simple script using the V1 action.
        /// </summary>
        [Benchmark]
        public async Task ExecuteScriptV1SimpleScript()
        {
            await _v1Action.ExecuteAsync(Context, Settings._v1SimpleSettings, CancellationToken);
        }

        /// <summary>
        /// Execute a simple script using the V2 action.
        /// </summary>
        [Benchmark]
        public async Task ExecuteScriptV2SimpleScript()
        {
            await _v2Action.ExecuteAsync(Context, Settings._v2SimpleSettings, CancellationToken);
        }

        /// <summary>
        /// Execute a recursion loop script using the V1 action.
        /// </summary>
        [Benchmark]
        public async Task ExecuteScriptV1RecursionLoopScript()
        {
            await _v1Action.ExecuteAsync(Context, Settings._v1RecursionLoopSettings,
                CancellationToken);
        }

        /// <summary>
        /// Execute a recursion loop script using the V2 action.
        /// </summary>
        [Benchmark]
        public async Task ExecuteScriptV2RecursionLoopScript()
        {
            await _v2Action.ExecuteAsync(Context, Settings._v2RecursionLoopSettings,
                CancellationToken);
        }
    }
}