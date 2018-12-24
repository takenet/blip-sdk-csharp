using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Action = Take.Blip.Builder.Models.Action;

namespace Take.Blip.Builder
{
    public class FlowManager : IFlowManager
    {
        private readonly IConfiguration _configuration;
        private readonly IStateManager _stateManager;
        private readonly IContextProvider _contextProvider;
        private readonly INamedSemaphore _namedSemaphore;
        private readonly IActionProvider _actionProvider;
        private readonly ISender _sender;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly IArtificialIntelligenceExtension _artificialIntelligenceExtension;
        private readonly IVariableReplacer _variableReplacer;
        private readonly ILogger _logger;
        private readonly ITraceProcessor _traceProcessor;

        private readonly ActionBlock<TraceEvent> _traceActionBlock;

        public FlowManager(
            IConfiguration configuration,
            IStateManager stateManager,
            IContextProvider contextProvider,
            INamedSemaphore namedSemaphore,
            IActionProvider actionProvider,
            ISender sender,
            IDocumentSerializer documentSerializer,
            IEnvelopeSerializer envelopeSerializer,
            IArtificialIntelligenceExtension artificialIntelligenceExtension,
            IVariableReplacer variableReplacer,
            ILogger logger,
            ITraceProcessor traceProcessor)
        {
            _configuration = configuration;
            _stateManager = stateManager;
            _contextProvider = contextProvider;
            _namedSemaphore = namedSemaphore;
            _actionProvider = actionProvider;
            _sender = sender;
            _documentSerializer = documentSerializer;
            _envelopeSerializer = envelopeSerializer;
            _artificialIntelligenceExtension = artificialIntelligenceExtension;
            _variableReplacer = variableReplacer;
            _logger = logger;
            _traceActionBlock = new ActionBlock<TraceEvent>(
                ProcessTraceAsync,
                new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = configuration.TraceQueueBoundedCapacity,
                    MaxDegreeOfParallelism = configuration.TraceQueueMaxDegreeOfParalelism,
                    EnsureOrdered = false
                });
            _traceProcessor = traceProcessor;
        }

        public async Task ProcessInputAsync(Document input, Identity user, Identity application, Flow flow, CancellationToken cancellationToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (flow == null)
            {
                throw new ArgumentNullException(nameof(flow));
            }

            flow.Validate();

            // Input tracing infrastructure
            InputTrace inputTrace = null;
            if (flow.TraceSettings != null &&
                flow.TraceSettings.Mode != TraceMode.Disabled)
            {
                inputTrace = new InputTrace
                {
                    FlowId = flow.Id,
                    User = user,
                    Input = input.ToString()
                };
            }

            var inputStopwatch = inputTrace != null
                ? Stopwatch.StartNew()
                : null;
            try
            {
                // Create a cancellation token
                using (var cts = new CancellationTokenSource(_configuration.InputProcessingTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                {
                    // Synchronize to avoid concurrency issues on multiple running instances
                    var handle = await _namedSemaphore.WaitAsync($"{flow.Id}:{user}", _configuration.InputProcessingTimeout, linkedCts.Token);
                    try
                    {
                        // Create the input evaluator
                        var lazyInput = new LazyInput(input, flow.Configuration, _documentSerializer,
                            _envelopeSerializer, _artificialIntelligenceExtension, linkedCts.Token);

                        // Load the user context
                        var context = _contextProvider.CreateContext(user, application, lazyInput, flow);

                        // Try restore a stored state
                        var stateId = await _stateManager.GetStateIdAsync(context, linkedCts.Token);
                        var state = flow.States.FirstOrDefault(s => s.Id == stateId) ?? flow.States.Single(s => s.Root);

                        // Calculate the number of state transitions
                        var transitions = 0;

                        do
                        {
                            // Create trace instances, if required
                            var (stateTrace, stateStopwatch) = inputTrace != null
                                ? (state.ToTrace(), Stopwatch.StartNew())
                                : (null, null);

                            try
                            {
                                linkedCts.Token.ThrowIfCancellationRequested();

                                // Validate the input for the current state
                                if (state.Input != null &&
                                    !state.Input.Bypass &&
                                    state.Input.Validation != null &&
                                    !ValidateDocument(lazyInput, state.Input.Validation))
                                {
                                    if (state.Input.Validation.Error != null)
                                    {
                                        // Send the validation error message
                                        await _sender.SendMessageAsync(state.Input.Validation.Error, user.ToNode(),
                                            linkedCts.Token);
                                    }

                                    break;
                                }

                                // Set the input in the context
                                if (!string.IsNullOrEmpty(state.Input?.Variable))
                                {
                                    await context.SetVariableAsync(state.Input.Variable, lazyInput.SerializedContent,
                                        linkedCts.Token);
                                }

                                // Prepare to leave the current state executing the output actions
                                if (state.OutputActions != null)
                                {
                                    await ProcessActionsAsync(lazyInput, context, state.OutputActions, stateTrace?.OutputActions, linkedCts.Token);
                                }

                                // Store the previous state and determine the next
                                await _stateManager.SetPreviousStateIdAsync(context, state.Id, cancellationToken);
                                state = await ProcessOutputsAsync(lazyInput, context, flow, state, stateTrace?.Outputs, linkedCts.Token);

                                // Store the next state
                                if (state != null)
                                {
                                    await _stateManager.SetStateIdAsync(context, state.Id, linkedCts.Token);
                                }
                                else
                                {
                                    await _stateManager.DeleteStateIdAsync(context, linkedCts.Token);
                                }

                                // Process the next state input actions
                                if (state?.InputActions != null)
                                {
                                    await ProcessActionsAsync(lazyInput, context, state.InputActions, stateTrace?.InputActions, linkedCts.Token);
                                }

                                // Check if the state transition limit has reached (to avoid loops in the flow)
                                if (transitions++ >= _configuration.MaxTransitionsByInput)
                                {
                                    throw new BuilderException("Max state transitions reached");
                                }
                            }
                            catch (Exception ex)
                            {
                                if (stateTrace != null)
                                {
                                    stateTrace.Error = ex.ToString();
                                }
                                throw;
                            }
                            finally
                            {
                                stateStopwatch?.Stop();
                                if (inputTrace != null &&
                                    stateTrace != null &&
                                    stateStopwatch != null)
                                {
                                    stateTrace.ElapsedMilliseconds = stateStopwatch.ElapsedMilliseconds;
                                    inputTrace.States.Add(stateTrace);
                                }
                            }

                            // Continue processing if the next has do not expect the user input
                        } while (state != null && (state.Input == null || state.Input.Bypass));
                    }
                    finally
                    {
                        await handle.DisposeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing input '{Input}' for user '{User}'", input, user);

                if (inputTrace != null)
                {
                    inputTrace.Error = ex.ToString();
                }

                throw;
            }
            finally
            {
                inputStopwatch?.Stop();

                // Check if we should trace the request
                if (inputTrace != null &&
                    flow.TraceSettings != null &&
                    inputStopwatch != null &&
                    (
                        flow.TraceSettings.Mode == TraceMode.All ||
                        (flow.TraceSettings.Mode.IsSlow() && inputStopwatch.ElapsedMilliseconds >= (flow.TraceSettings.SlowThreshold ?? 5000)) ||
                        (flow.TraceSettings.Mode.IsError() && inputTrace.Error != null)
                    ))
                {
                    inputTrace.ElapsedMilliseconds = inputStopwatch.ElapsedMilliseconds;
                    await _traceActionBlock.SendAsync(
                        new TraceEvent
                        {
                            Trace = inputTrace,
                            Settings = flow.TraceSettings
                        },
                        cancellationToken);
                }
            }
        }

        private bool ValidateDocument(LazyInput lazyInput, InputValidation inputValidation)
        {
            switch (inputValidation.Rule)
            {
                case InputValidationRule.Text:
                    return lazyInput.Content is PlainText;

                case InputValidationRule.Number:
                    return decimal.TryParse(lazyInput.SerializedContent, out _);

                case InputValidationRule.Date:
                    return DateTime.TryParse(lazyInput.SerializedContent, out _);

                case InputValidationRule.Regex:
                    return Regex.IsMatch(lazyInput.SerializedContent, inputValidation.Regex);

                case InputValidationRule.Type:
                    return lazyInput.Content.GetMediaType() == inputValidation.Type;

                default:
                    throw new ArgumentOutOfRangeException(nameof(inputValidation));
            }
        }

        private async Task ProcessActionsAsync(LazyInput lazyInput, IContext context, Action[] actions, ICollection<ActionTrace> actionTraces, CancellationToken cancellationToken)
        {
            // Execute all state actions
            foreach (var stateAction in actions.OrderBy(a => a.Order))
            {
                 var isValidAction = await EvaluateConditionsAsync(stateAction.Conditions, lazyInput, context, cancellationToken);
                if (isValidAction)
                {
                    var action = _actionProvider.Get(stateAction.Type);

                    // Trace infra
                    var (actionTrace, actionStopwatch) = actionTraces != null
                        ? (stateAction.ToTrace(), Stopwatch.StartNew())
                        : (null, null);

                    try
                    {
                        var settings = stateAction.Settings;
                        if (settings != null)
                        {
                            var settingsJson = settings.ToString(Formatting.None);
                            settingsJson = await _variableReplacer.ReplaceAsync(settingsJson, context, cancellationToken);
                            settings = JObject.Parse(settingsJson);
                        }

                        if (actionTrace != null)
                        {
                            actionTrace.ParsedSettings = settings;
                        }

                        await action.ExecuteAsync(context, settings, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        if (actionTrace != null)
                        {
                            actionTrace.Error = ex.ToString();
                        }

                        throw new ActionProcessingException(
                            $"The processing of the action '{stateAction.Type}' has failed: {ex.Message}", ex);
                    }
                    finally
                    {
                        actionStopwatch?.Stop();

                        if (actionTrace != null &&
                            actionTraces != null &&
                            actionStopwatch != null)
                        {
                            actionTrace.ElapsedMilliseconds = actionStopwatch.ElapsedMilliseconds;
                            actionTraces.Add(actionTrace);
                        }
                    }
                }
            }
        }

        private async Task<State> ProcessOutputsAsync(LazyInput lazyInput, IContext context, Flow flow, State state, ICollection<OutputTrace> outputTraces, CancellationToken cancellationToken)
        {
            var outputs = state.Outputs;
            state = null;

            // If there's any output in the current state
            if (outputs != null)
            {
                // Evalute each output conditions
                foreach (var output in outputs.OrderBy(o => o.Order))
                {
                    var (outputTrace, outputStopwatch) = outputTraces != null
                        ? (output.ToTrace(), Stopwatch.StartNew())
                        : (null, null);

                    try
                    {
                         var isValidOutput = await EvaluateConditionsAsync(output.Conditions, lazyInput, context, cancellationToken);

                        if (isValidOutput)
                        {
                            state = flow.States.SingleOrDefault(s => s.Id == output.StateId);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (outputTrace != null)
                        {
                            outputTrace.Error = ex.ToString();
                        }
                        throw;
                    }
                    finally
                    {
                        outputStopwatch?.Stop();

                        if (outputTrace != null &&
                            outputTraces != null &&
                            outputStopwatch != null)
                        {
                            outputTrace.ElapsedMilliseconds = outputStopwatch.ElapsedMilliseconds;
                            outputTraces.Add(outputTrace);
                        }
                    }
                }
            }

            return state;
        }

         private async Task<bool> EvaluateConditionsAsync(
            IEnumerable<Condition> conditions,
            LazyInput lazyInput,
            IContext context,
            CancellationToken cancellationToken)
        {
            var isValidOutput = true;

            if (conditions != null)
            {
                foreach (var outputCondition in conditions)
                {
                    isValidOutput = await EvaluateConditionAsync(outputCondition, lazyInput, context, cancellationToken);
                    if (!isValidOutput)
                    {
                        return false;
                    }
                }
            }

            return isValidOutput;
        }

        private async Task<bool> EvaluateConditionAsync(
            Condition condition,
            LazyInput lazyInput,
            IContext context,
            CancellationToken cancellationToken)
        {
            string comparisonValue;

            switch (condition.Source)
            {
                case ValueSource.Input:
                    comparisonValue = lazyInput.SerializedContent;
                    break;

                case ValueSource.Context:
                    comparisonValue = await context.GetVariableAsync(condition.Variable, cancellationToken);
                    break;

                case ValueSource.Intent:
                    comparisonValue = (await lazyInput.GetIntentAsync())?.Name;
                    break;

                case ValueSource.Entity:
                    comparisonValue = (await lazyInput.GetEntityValue(condition.Entity))?.Value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (condition.Comparison.GetComparisonType())
            {
                case ComparisonType.Unary:
                    var unaryComparisonFunc = condition.Comparison.ToUnaryDelegate();

                    return unaryComparisonFunc(comparisonValue);

                case ComparisonType.Binary:
                    var binaryComparisonFunc = condition.Comparison.ToBinaryDelegate();

                    switch (condition.Operator)
                    {
                        case ConditionOperator.Or:
                            return condition.Values.Any(v => binaryComparisonFunc(comparisonValue, v));

                        case ConditionOperator.And:
                            return condition.Values.All(v => binaryComparisonFunc(comparisonValue, v));

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task ProcessTraceAsync(TraceEvent traceEvent)
        {
            try
            {
                using (var cts = new CancellationTokenSource(_configuration.TraceProcessingTimeout))
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
