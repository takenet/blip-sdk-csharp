using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Diagnostics
{
    public class TraceManager : ITraceManager
    {
        private readonly ITraceProcessor _traceProcessor;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly ActionBlock<TraceEvent> _traceActionBlock;

        public TraceManager(
            ILogger logger,
            ITraceProcessor traceProcessor,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _traceProcessor = traceProcessor;
            _logger = logger;
            _traceActionBlock = new ActionBlock<TraceEvent>(
                ProcessTraceAsync,
                new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = configuration.TraceQueueBoundedCapacity,
                    MaxDegreeOfParallelism = configuration.TraceQueueMaxDegreeOfParallelism,
                    EnsureOrdered = false
                });
        }

        public (StateTrace, Stopwatch) CreateStateTrace(InputTrace inputTrace, State state, StateTrace stateTrace = null, Stopwatch stateStopwatch = null)
        {
            stateStopwatch?.Stop();
            if (inputTrace != null &&
                stateTrace != null &&
                stateStopwatch != null)
            {
                stateTrace.ElapsedMilliseconds = stateStopwatch.ElapsedMilliseconds;
                inputTrace.States.Add(stateTrace);
            }

            if (state != null && inputTrace != null)
            {
                (stateTrace, stateStopwatch) = (state.ToTrace(), Stopwatch.StartNew());
            }
            else
            {
                (stateTrace, stateStopwatch) = (null, null);
            }

            return (stateTrace, stateStopwatch);
        }

        public async Task ProcessTraceAsync(InputTrace inputTrace, TraceSettings traceSettings, Stopwatch inputStopwatch, CancellationToken cancellationToken)
        {
            inputStopwatch?.Stop();

            // Check if we should trace the request
            if (inputTrace != null &&
                traceSettings != null &&
                inputStopwatch != null &&
                (
                    traceSettings.Mode == TraceMode.All ||
                    (traceSettings.Mode.IsSlow() && inputStopwatch.ElapsedMilliseconds >= (traceSettings.SlowThreshold ?? 5000)) ||
                    (traceSettings.Mode.IsError() && inputTrace.Error != null)
                ))
            {
                inputTrace.ElapsedMilliseconds = inputStopwatch.ElapsedMilliseconds;
                await _traceActionBlock.SendAsync(
                    new TraceEvent
                    {
                        Trace = inputTrace,
                        Settings = traceSettings
                    },
                    cancellationToken);
            }
        }

        private async Task ProcessTraceAsync(TraceEvent traceEvent)
        {
            try
            {
                using (var cts = new CancellationTokenSource(_configuration.TraceTimeout))
                {
                    await _traceProcessor.ProcessTraceAsync(traceEvent, cts.Token);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Error tracing trace event for user '{User}' and input '{Input}'",
                    traceEvent?.Trace?.User,
                    traceEvent?.Trace?.Input);
            }
        }
    }
}