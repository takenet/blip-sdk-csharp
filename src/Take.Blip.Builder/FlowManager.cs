using Lime.Messaging.Contents;
using Lime.Messaging.Resources;
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
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;
using Take.Blip.Client.Activation;
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
        private readonly ITraceManager _traceManager;
        private readonly IUserOwnerResolver _userOwnerResolver;
        private readonly Identity _applicationIdentity;
        
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
            ITraceManager traceManager,
            IUserOwnerResolver userOwnerResolver, 
            Application application)
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
            _traceManager = traceManager;
            _userOwnerResolver = userOwnerResolver;
            _applicationIdentity = application.Identity;
        }

        public async Task ProcessInputAsync(Message message, Flow flow, Contact contact, CancellationToken cancellationToken)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.From == null)
            {
                throw new ArgumentException("Message 'from' must be present", nameof(message));
            }

            if (flow == null)
            {
                throw new ArgumentNullException(nameof(flow));
            }

            flow.Validate();
            
            // Determine the user / owner pair
            var (userIdentity, ownerIdentity) = await _userOwnerResolver.GetUserOwnerIdentitiesAsync(message, flow.BuilderConfiguration, cancellationToken);

            // Input tracing infrastructure
            InputTrace inputTrace = null;
            TraceSettings traceSettings;
            
            if (message.Metadata != null &&
                message.Metadata.Keys.Contains(TraceSettings.BUILDER_TRACE_TARGET))
            {
                traceSettings = new TraceSettings(message.Metadata);
            }
            else
            {
                traceSettings = flow.TraceSettings;
            }

            if (traceSettings != null &&
                traceSettings.Mode != TraceMode.Disabled)
            {
                inputTrace = new InputTrace
                {
                    FlowId = flow.Id,
                    User = userIdentity,
                    Input = message.Content.ToString()
                };
            }

            var inputStopwatch = inputTrace != null
                ? Stopwatch.StartNew()
                : null;

            // Sets the owner context if required
            IDisposable ownerContext = null;
            if (ownerIdentity != _applicationIdentity)
            {
                ownerContext = OwnerContext.Create(ownerIdentity);
            }
            
            try
            {
                // Create a cancellation token
                using (var cts = new CancellationTokenSource(_configuration.InputProcessingTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                {
                    // Synchronize to avoid concurrency issues on multiple running instances
                    var handle = await _namedSemaphore.WaitAsync($"{flow.Id}:{userIdentity}", _configuration.InputProcessingTimeout, linkedCts.Token);
                    try
                    {
                        // Create the input evaluator
                        var lazyInput = new LazyInput(message, flow.BuilderConfiguration, _documentSerializer,
                            _envelopeSerializer, _artificialIntelligenceExtension, linkedCts.Token);

                        // Load the user context
                        var context = _contextProvider.CreateContext(userIdentity, ownerIdentity, lazyInput, flow);
                        if (contact != null)
                        {
                            context.SetContact(contact);
                        }

                        // Try restore a stored state
                        var stateId = await _stateManager.GetStateIdAsync(context, linkedCts.Token);
                        var state = flow.States.FirstOrDefault(s => s.Id == stateId) ?? flow.States.Single(s => s.Root);

                        // Calculate the number of state transitions
                        var transitions = 0;

                        // Create trace instances, if required
                        var (stateTrace, stateStopwatch) = _traceManager.CreateStateTrace(inputTrace, state);
                        
                        // Process the global input actions
                        if (flow.InputActions != null)
                        {
                            await ProcessActionsAsync(lazyInput, context, flow.InputActions, inputTrace?.InputActions, linkedCts.Token);
                        }

                        var stateWaitForInput = true;
                        do
                        {
                            try
                            {
                                linkedCts.Token.ThrowIfCancellationRequested();

                                // Validate the input for the current state
                                if (stateWaitForInput &&
                                    state.Input?.Validation != null &&
                                    !ValidateDocument(lazyInput, state.Input.Validation))
                                {
                                    if (state.Input.Validation.Error != null)
                                    {
                                        // Send the validation error message
                                        await _sender.SendMessageAsync(state.Input.Validation.Error, userIdentity.ToNode(),
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

                                // Create trace instances, if required
                                (stateTrace, stateStopwatch) = _traceManager.CreateStateTrace(inputTrace, state, stateTrace, stateStopwatch);

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
                                // Continue processing if the next state do not expect the user input
                                var inputConditionIsValid = state?.Input?.Conditions == null || 
                                                            await state.Input.Conditions.EvaluateConditionsAsync(lazyInput, context, cancellationToken);
                                stateWaitForInput = state == null || 
                                                    (state.Input != null && !state.Input.Bypass && inputConditionIsValid);
                                if (stateWaitForInput)
                                {
                                    // Create a new trace if the next state waits for an input     
                                    (stateTrace, stateStopwatch) = _traceManager.CreateStateTrace(inputTrace, state, stateTrace, stateStopwatch);
                                }
                            }
                        } while (!stateWaitForInput);
                        
                        // Process the global output actions
                        if (flow.OutputActions != null)
                        {
                            await ProcessActionsAsync(lazyInput, context, flow.OutputActions, inputTrace?.OutputActions, linkedCts.Token);
                        }
                    }
                    finally
                    {
                        await handle.DisposeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing input '{Input}' for user '{User}'", message.Content, userIdentity);

                if (inputTrace != null)
                {
                    inputTrace.Error = ex.ToString();
                }
                
                ownerContext?.Dispose();

                throw;
            }
            finally
            {
                using (var cts = new CancellationTokenSource(_configuration.TraceTimeout))
                {
                    await _traceManager.ProcessTraceAsync(inputTrace, traceSettings, inputStopwatch, cts.Token);
                }
            }
        }

        private static bool ValidateDocument(LazyInput lazyInput, InputValidation inputValidation)
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
                if (stateAction.Conditions != null &&
                    !await stateAction.Conditions.EvaluateConditionsAsync(lazyInput, context, cancellationToken))
                {
                    continue;
                }

                var action = _actionProvider.Get(stateAction.Type);

                // Trace infra
                var (actionTrace, actionStopwatch) = actionTraces != null
                    ? (stateAction.ToTrace(), Stopwatch.StartNew())
                    : (null, null);

                // Configure the action timeout, that can be defined in action or flow level
                var executionTimeoutInSeconds =
                    stateAction.Timeout ?? context.Flow?.BuilderConfiguration?.ActionExecutionTimeout;

                var executionTimeout = executionTimeoutInSeconds.HasValue
                    ? TimeSpan.FromSeconds(executionTimeoutInSeconds.Value)
                    : _configuration.DefaultActionExecutionTimeout;
                        
                using (var cts = new CancellationTokenSource(executionTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                {
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
                        
                        await action.ExecuteAsync(context, settings, linkedCts.Token);
                    }
                    catch (Exception ex)
                    {
                        if (actionTrace != null)
                        {
                            actionTrace.Error = ex.ToString();
                        }

                        if (!stateAction.ContinueOnError)
                        {
                            var message = ex is OperationCanceledException && cts.IsCancellationRequested
                                ? $"The processing of the action '{stateAction.Type}' has timed out after {executionTimeout.TotalMilliseconds} ms"
                                : $"The processing of the action '{stateAction.Type}' has failed: {ex.Message}";
                                
                            throw new ActionProcessingException(message, ex);
                        }
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
                        if (output.Conditions == null ||
                            await output.Conditions.EvaluateConditionsAsync(lazyInput, context, cancellationToken))
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
    }
}