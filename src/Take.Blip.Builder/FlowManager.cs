using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private readonly Take.Blip.Client.Session.IStateManager _stateSessionManager;
        private readonly IFlowLoader _flowLoader;
        private readonly IContextProvider _contextProvider;
        private readonly IFlowSemaphore _flowSemaphore;

        private readonly IActionProvider _actionProvider;
        private readonly ISender _sender;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly IArtificialIntelligenceExtension _artificialIntelligenceExtension;
        private readonly IVariableReplacer _variableReplacer;
        private readonly ILogger _logger;
        private readonly ITraceManager _traceManager;
        private readonly IUserOwnerResolver _userOwnerResolver;
        private readonly IInputExpirationHandler _inputExpirationHandler;
        private readonly Node _applicationNode;

        private const string SHORTNAME_OF_SUBFLOW_EXTENSION_DATA = "shortNameOfSubflow";

        public FlowManager(
            IConfiguration configuration,
            IStateManager stateManager,
            IContextProvider contextProvider,
            IFlowSemaphore flowSemaphore,
            IActionProvider actionProvider,
            ISender sender,
            IDocumentSerializer documentSerializer,
            IEnvelopeSerializer envelopeSerializer,
            IArtificialIntelligenceExtension artificialIntelligenceExtension,
            IVariableReplacer variableReplacer,
            ILogger logger,
            ITraceManager traceManager,
            IUserOwnerResolver userOwnerResolver,
            IInputExpirationHandler inputExpirationHandler,
            Application application, 
            Client.Session.IStateManager stateSessionManager,
            IFlowLoader flowLoader
            )
        {
            _configuration = configuration;
            _stateManager = stateManager;
            _contextProvider = contextProvider;
            _flowSemaphore = flowSemaphore;
            _actionProvider = actionProvider;
            _sender = sender;
            _documentSerializer = documentSerializer;
            _envelopeSerializer = envelopeSerializer;
            _artificialIntelligenceExtension = artificialIntelligenceExtension;
            _variableReplacer = variableReplacer;
            _logger = logger;
            _traceManager = traceManager;
            _userOwnerResolver = userOwnerResolver;
            _inputExpirationHandler = inputExpirationHandler;
            _applicationNode = application.Node;
            _stateSessionManager = stateSessionManager;
            _flowLoader = flowLoader;
        }

        public async Task ProcessInputAsync(Message message, Flow flow, CancellationToken cancellationToken)
        {
            await ProcessInputAsync(message, flow, null, cancellationToken);
        }

        public async Task ProcessInputAsync(Message message, Flow flow, IContext messageContext, CancellationToken cancellationToken)
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

            var (messageHasChanged, newMessage) = _inputExpirationHandler.ValidateMessage(message);

            // If the message has changedm the old context can't be used because it has the old message.
            // Setting it to null will force a new context to be created later with the new message.
            if (messageHasChanged)
            {
                messageContext = null;
                message = newMessage;
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
                    Owner = ownerIdentity,
                    FlowId = flow.Id,
                    User = userIdentity,
                    Input = message.Content.ToString()
                };
            }

            var inputStopwatch = inputTrace != null
                ? Stopwatch.StartNew()
                : null;

            var ownerContext = OwnerContext.Create(ownerIdentity);

            State state = default;
            try
            {
                // Create a cancellation token
                using (var cts = new CancellationTokenSource(_configuration.InputProcessingTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                {
                    // Synchronize to avoid concurrency issues on multiple running instances
                    var handle = await _flowSemaphore.WaitAsync(flow, message, userIdentity, _configuration.InputProcessingTimeout, linkedCts.Token);
                    try
                    {
                        // Create the input evaluator
                        var lazyInput = new LazyInput(message, userIdentity, flow.BuilderConfiguration, _documentSerializer,
                            _envelopeSerializer, _artificialIntelligenceExtension, linkedCts.Token);

                        // Load the user context
                        var context = messageContext ?? _contextProvider.CreateContext(userIdentity, ownerIdentity, lazyInput, flow);

                        // Try restore a stored state
                        var stateId = await _stateManager.GetStateIdAsync(context, linkedCts.Token);
                        var previousState = await _stateManager.GetPreviousStateIdAsync(context, linkedCts.Token);

                        state = flow.States.FirstOrDefault(s => s.Id == stateId) ?? flow.States.Single(s => s.Root);

                        // If current stateId of user is different of inputExpiration stop processing
                        if (!_inputExpirationHandler.IsValidateState(state, message))
                        {
                            return;
                        }

                        await _inputExpirationHandler.OnFlowPreProcessingAsync(state, message, _applicationNode, linkedCts.Token);

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
                        var parentStateIdQueue = new Queue<string>();
                        do
                        {
                            var redirectToClientState = String.Empty;
                            try
                            {
                                linkedCts.Token.ThrowIfCancellationRequested();

                                if (stateWaitForInput)
                                {
                                    // Validate the input for the current state
                                    if (state.Input?.Validation != null && !lazyInput.SerializedContent.IsNullOrEmpty() &&
                                        !ValidateDocument(lazyInput, state.Input.Validation))
                                    {
                                        if (state.Input.Validation.Error != null)
                                        {
                                            // Send the validation error message
                                            if (IsContextVariable(state.Input.Validation.Error))
                                            {
                                                var validationMessage = new Message(null)
                                                {
                                                    To = context.Input.Message.From
                                                };
                                                validationMessage.Metadata = new Dictionary<string, string>
                                                {  { "#message.spinText", "true"} };

                                                validationMessage.Content = new PlainDocument(state.Input.Validation.Error, MediaType.TextPlain);
                                                await _sender.SendMessageAsync(validationMessage, linkedCts.Token);
                                            }
                                            else
                                            {
                                                await _sender.SendMessageAsync(state.Input.Validation.Error, message.From, linkedCts.Token);
                                            }
                                        }

                                        break;
                                    }

                                    // Set the input in the context
                                    if (!string.IsNullOrEmpty(state.Input?.Variable))
                                    {
                                        await context.SetVariableAsync(state.Input.Variable, lazyInput.SerializedContent,
                                            linkedCts.Token);
                                    }
                                }

                                // Prepare to leave the current state executing the output actions
                                if (state.OutputActions != null)
                                {
                                    await ProcessActionsAsync(lazyInput, context, state.OutputActions, stateTrace?.OutputActions, linkedCts.Token);
                                }

                                var previousStateId = state.Id;
                                if (IsContextVariable(state.Id))
                                {
                                    previousStateId = await _variableReplacer.ReplaceAsync(state.Id, context, linkedCts.Token);
                                }

                                if (!state.End)
                                {
                                    // Determine the next state
                                    state = await ProcessOutputsAsync(lazyInput, context, flow, state, stateTrace?.Outputs, linkedCts.Token);

                                    // Store the previous state
                                    await _stateManager.SetPreviousStateIdAsync(context, previousStateId, linkedCts.Token);

                                    if (IsSubflowState(state))
                                    {
                                        // Create trace instances, if required
                                        (stateTrace, stateStopwatch) = _traceManager.CreateStateTrace(inputTrace, state, stateTrace, stateStopwatch);

                                        // Process the next state input actions
                                        if (state?.InputActions != null)
                                        {
                                            await ProcessActionsAsync(lazyInput, context, state.InputActions, stateTrace?.InputActions, linkedCts.Token);
                                        }

                                        parentStateIdQueue.Enqueue(state.Id);
                                        await _stateManager.SetStateIdAsync(context, state.Id, linkedCts.Token);
                                        (flow, state) = await RedirectToSubflowAsync(context, userIdentity, state, flow, linkedCts.Token);
                                    }
                                }
                                else
                                {
                                    await _stateManager.SetPreviousStateIdAsync(context, previousStateId, linkedCts.Token);
                                    await _stateManager.DeleteStateIdAsync(context, linkedCts.Token);

                                    (flow, state) = await RedirectToParentFlowAsync(
                                        context, 
                                        userIdentity, 
                                        flow, 
                                        await GetParentStateIdAsync(context, parentStateIdQueue, linkedCts.Token),
                                        linkedCts.Token
                                    );

                                    // Create trace instances, if required
                                    (stateTrace, stateStopwatch) = _traceManager.CreateStateTrace(inputTrace, state);

                                    // Prepare to leave the current state executing the output actions
                                    if (state.OutputActions != null)
                                    {
                                        await ProcessActionsAsync(lazyInput, context, state.OutputActions, stateTrace?.OutputActions, linkedCts.Token);
                                    }

                                    state = await ProcessOutputsAsync(lazyInput, context, flow, state, stateTrace?.Outputs, linkedCts.Token);
                                }

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
                                    throw new BuilderException($"Max state transitions of {_configuration.MaxTransitionsByInput} was reached");
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
                                if (stateTrace?.Error != null || stateWaitForInput)
                                {
                                    // Create a new trace if the next state waits for an input or the state without an input throws an error     
                                    (stateTrace, stateStopwatch) = _traceManager.CreateStateTrace(inputTrace, state, stateTrace, stateStopwatch);
                                }
                            }
                        } while (!stateWaitForInput);

                        await ProcessGlobalOutputActionsAsync(context, flow, lazyInput, inputTrace, linkedCts.Token);

                        await _inputExpirationHandler.OnFlowProcessedAsync(state, message, _applicationNode, linkedCts.Token);
                    }
                    finally
                    {
                        await handle?.DisposeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                if (inputTrace != null)
                {
                    inputTrace.Error = ex.ToString();
                }

                var builderException = ex is BuilderException be ? be :
                    new BuilderException($"Error processing input '{message.Content}' for user '{userIdentity}' in state '{state?.Id}'", ex);

                builderException.StateId = state?.Id;
                builderException.UserId = userIdentity;
                builderException.MessageId = message.Id;

                throw builderException;
            }
            finally
            {
                using (var cts = new CancellationTokenSource(_configuration.TraceTimeout))
                {
                    await _traceManager.ProcessTraceAsync(inputTrace, traceSettings, inputStopwatch, cts.Token);
                }

                ownerContext.Dispose();
            }
        }

        private async Task ProcessGlobalOutputActionsAsync(IContext context, Flow flow, LazyInput lazyInput, InputTrace inputTrace, CancellationToken cancellationToken)
        {
            if (flow.OutputActions != null)
            {
                await ProcessActionsAsync(lazyInput, context, flow.OutputActions, inputTrace?.OutputActions, cancellationToken);
            }
        }

        private async Task<(Flow, State)> RedirectToSubflowAsync(IContext context, Identity userIdentity, State state, Flow parentFlow, CancellationToken cancellationToken)
        {
            var shortNameOfSubflow = state.GetExtensionDataValue(SHORTNAME_OF_SUBFLOW_EXTENSION_DATA);
            if (shortNameOfSubflow.IsNullOrEmpty())
            {
                throw new ArgumentNullException($"Error on redirect to subflow '{state.Id}'");
            }

            var subflow = await _flowLoader.LoadFlowAsync(FlowType.Subflow, parentFlow, shortNameOfSubflow, cancellationToken);
            if (subflow == null)
            {
                throw new ArgumentNullException($"Error on return subflow '{shortNameOfSubflow}'");
            }

            subflow.Validate();
            context.Flow = subflow;
            var newState = subflow.States.Single(s => s.Root);

            await _stateSessionManager.SetStateAsync(userIdentity, shortNameOfSubflow, cancellationToken);

            return (subflow, newState);
        }

        private async Task<(Flow, State)> RedirectToParentFlowAsync(IContext context, Identity userIdentity, Flow flow, string parentStateId, CancellationToken cancellationToken)
        {
            var parentFlow = flow.Parent;

            if (parentFlow == null)
            {
                throw new ArgumentNullException($"Error on return to parent flow of '{flow.Id}'");
            }

            context.Flow = parentFlow;
            var state = parentFlow.States.FirstOrDefault(s => s.Id == parentStateId) ?? parentFlow.States.Single(s => s.Root);

            await _stateSessionManager.SetStateAsync(userIdentity, parentFlow.SessionState, cancellationToken);

            return (parentFlow, state);
        }

        private bool IsSubflowState(State state) => state != null && state.Id.StartsWith("subflow:");

        private static bool ValidateDocument(LazyInput lazyInput, InputValidation inputValidation)
        {
            switch (inputValidation.Rule)
            {
                case InputValidationRule.Text:
                    return lazyInput.Content is PlainText;

                case InputValidationRule.Number:
                    return decimal.TryParse(lazyInput.SerializedContent, out _);

                case InputValidationRule.Date:
                    return DateTime.TryParseExact(lazyInput.SerializedContent, Constants.DateValidationFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out _);

                case InputValidationRule.Regex:
                    return Regex.IsMatch(lazyInput.SerializedContent, inputValidation.Regex, default, Constants.REGEX_TIMEOUT);

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

                if (actionTrace != null)
                {
                    context.SetCurrentActionTrace(actionTrace);
                }

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

                        using (LogContext.PushProperty(nameof(BuilderException.MessageId), lazyInput?.Message?.Id))
                        using (LogContext.PushProperty(nameof(Action.Settings), settings, true))
                            await action.ExecuteAsync(context, settings, linkedCts.Token);
                    }
                    catch (Exception ex)
                    {
                        if (actionTrace != null)
                        {
                            actionTrace.Error = ex.ToString();
                        }

                        var message = ex is OperationCanceledException && cts.IsCancellationRequested
                            ? $"The processing of the action '{stateAction.Type}' has timed out after {executionTimeout.TotalMilliseconds} ms"
                            : $"The processing of the action '{stateAction.Type}' has failed";

                        var actionProcessingException = new ActionProcessingException(message, ex)
                        {
                            ActionType = stateAction.Type,
                            ActionSettings = stateAction.Settings.ToObject<IDictionary<string, object>>()
                        };

                        if (stateAction.ContinueOnError)
                        {
                            _logger.Warning(actionProcessingException, "Action '{ActionType}' has failed but was forgotten", stateAction.Type);
                        }
                        else
                        {
                            throw actionProcessingException;
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

        private Boolean IsContextVariable(string stateId)
        {
            return (stateId.StartsWith("{{") && stateId.EndsWith("}}"));
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
                            var replacedVariable = output.StateId;

                            if (IsContextVariable(replacedVariable))
                            {
                                replacedVariable = await _variableReplacer.ReplaceAsync(replacedVariable, context, cancellationToken);
                            }
                            state = flow.States.FirstOrDefault(s => s.Id == replacedVariable);

                            if (state == null)
                            {
                                await _stateManager.DeleteStateIdAsync(context, cancellationToken);

                                throw new InvalidOperationException($"Failed to process output condition, bacause the output context variable '{output.StateId}' is undefined or does not exist in the context.");
                            }

                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (outputTrace != null)
                        {
                            outputTrace.Error = ex.ToString();
                        }

                        throw new OutputProcessingException($"Failed to process output condition to state '{output.StateId}'", ex)
                        {
                            OutputStateId = output.StateId,
                            OutputConditions = output.Conditions
                        };
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

        private async Task<string> GetParentStateIdAsync(IContext context, Queue<string> parentStateIdQueue, CancellationToken cancellationToken) => parentStateIdQueue.Count > 0 ? parentStateIdQueue.Dequeue() : await _stateManager.GetParentStateIdAsync(context, cancellationToken);
    }

    static class StateExtensions
    {
        public static string GetExtensionDataValue(this State value, string key)
        {
            value.ExtensionData.TryGetValue(key, out var extensionData);
            return extensionData.ToString();
        }

    }
}