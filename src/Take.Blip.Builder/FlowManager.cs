﻿using Lime.Messaging.Contents;
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
using Take.Blip.Builder.Actions.ExecuteTemplate;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
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
        private readonly IFlowLoader _flowLoader;
        private readonly IFlowSessionManager _flowSessionManager;
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
        private readonly Node _applicationNode;
        private readonly IAnalyzeBuilderExceptions _analyzeBuilderExceptions;
        private readonly IInputMessageHandler _inputMessageHandlerAggregator;
        private readonly IInputExpirationCount _inputExpirationCount;

        private const string SHORTNAME_OF_SUBFLOW_EXTENSION_DATA = "shortNameOfSubflow";
        private const string ACTION_PROCESS_HTTP = "ProcessHttp";
        private const string ACTION_EXECUTE_TEMPLATE = "ExecuteTemplate";
        public const string STATE_ID = "inputExpiration.stateId";
        private const string START_SOURCE_TAKE_BLIP = "take.blip";
        private const string STATE_TRACE_INTERNAL_SERVER_ERROR = "Internal Server Error";

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
            Application application,
            IFlowLoader flowLoader,
            IFlowSessionManager flowSessionManager,
            IAnalyzeBuilderExceptions analyzeBuilderExceptions,
            IInputMessageHandlerAggregator inputMessageHandlerAggregator,
            IInputExpirationCount inputExpirationCount
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
            _applicationNode = application.Node;
            _flowLoader = flowLoader;
            _flowSessionManager = flowSessionManager;
            _analyzeBuilderExceptions = analyzeBuilderExceptions;
            _inputMessageHandlerAggregator = inputMessageHandlerAggregator;
            _inputExpirationCount = inputExpirationCount;
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
            ClearInputExpirationCount(message);
            var (messageHasChanged, newMessage) = _inputMessageHandlerAggregator.HandleMessage(message);

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

                        state = flow.States.FirstOrDefault(s => s.Id == stateId) ?? flow.States.Single(s => s.Root);

                        // If current stateId of user is different of inputExpiration stop processing
                        if (!_inputMessageHandlerAggregator.IsValidateState(state, message, flow))
                        {
                            return;
                        }

                        await _inputMessageHandlerAggregator.OnFlowPreProcessingAsync(state, message, _applicationNode, linkedCts.Token);

                        // Calculate the number of state transitions
                        var transitions = 0;

                        // Create trace instances, if required
                        var (stateTrace, stateStopwatch) = _traceManager.CreateStateTrace(inputTrace, state);

                        // Process the global input actions
                        if (flow.InputActions != null)
                        {
                            await ProcessActionsAsync(lazyInput, context, flow.InputActions, inputTrace?.InputActions, state, linkedCts.Token);
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
                                    if (!await ValidateInputAsync(message, state, lazyInput, context, linkedCts))
                                    {
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
                                await ProcessStateOutputActionsAsync(state, lazyInput, context, stateTrace, linkedCts.Token);

                                var previousStateId = state.Id;
                                var previousState = state;
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

                                    // Only execute the ProcessAfterStateActionsAsync when the user current state changed after ProcessOutputsAsync
                                    if (previousState.Id != state?.Id)
                                    {
                                        await ProcessAfterStateChangedActionsAsync(previousState, lazyInput, context, stateTrace, linkedCts.Token);
                                        await ProcessGlobalAfterStateChangedActionsAsync(context, flow, lazyInput, inputTrace, state, linkedCts.Token);
                                    }
                                }
                                else
                                {
                                    (flow, state, stateTrace, stateStopwatch) = await RedirectToParentFlowAsync(
                                        context,
                                        userIdentity,
                                        flow,
                                        previousStateId,
                                        await GetParentStateIdAsync(context, parentStateIdQueue, linkedCts.Token),
                                        inputTrace,
                                        lazyInput,
                                        linkedCts.Token
                                    );
                                }

                                if (IsSubflowState(state))
                                {
                                    parentStateIdQueue.Enqueue(state.Id);

                                    (flow, state, stateTrace, stateStopwatch) = await RedirectToSubflowAsync(
                                        context,
                                        userIdentity,
                                        state,
                                        flow,
                                        stateTrace,
                                        stateStopwatch,
                                        inputTrace,
                                        lazyInput,
                                        linkedCts.Token
                                   );
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
                                await ProcessStateInputActionsAsync(state, lazyInput, context, stateTrace, linkedCts.Token);

                                // Check if the state transition limit has reached (to avoid loops in the flow)
                                if (transitions++ >= _configuration.MaxTransitionsByInput)
                                {
                                    throw new FlowConstructionException($"Max state transitions of {_configuration.MaxTransitionsByInput} was reached");
                                }
                            }
                            catch (Exception ex)
                            {
                                if (stateTrace != null)
                                {
                                    if (ex.InnerException != null && !ex.InnerException.Source.ToLower().StartsWith(START_SOURCE_TAKE_BLIP))
                                    {
                                        stateTrace.Error = STATE_TRACE_INTERNAL_SERVER_ERROR;
                                    }
                                    else
                                    {
                                        stateTrace.Error = ex.ToString();
                                    }
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

                        await ProcessGlobalOutputActionsAsync(context, flow, lazyInput, inputTrace, state, linkedCts.Token);

                        await _inputMessageHandlerAggregator.OnFlowProcessedAsync(state, flow, message, _applicationNode, context, linkedCts.Token);
                    }
                    finally
                    {
                        await handle?.DisposeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                ex = _analyzeBuilderExceptions.VerifyFlowConstructionException(ex);

                if (inputTrace != null)
                {
                    inputTrace.Error = ex.ToString();
                }

                var builderException = ex is BuilderException be ? be :
                    new BuilderException($"Error processing input with message ID '{message.Id}' for user '{userIdentity}' in state '{state?.Id}'", ex);

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

        private async Task ProcessStateInputActionsAsync(State state, LazyInput lazyInput, IContext context, StateTrace stateTrace, CancellationToken cancellationToken)
        {
            if (state?.InputActions == null)
            {
                return;
            }

            await ProcessActionsAsync(lazyInput, context, state.InputActions, stateTrace?.InputActions, state, cancellationToken);
        }

        private async Task ProcessStateOutputActionsAsync(State state, LazyInput lazyInput, IContext context, StateTrace stateTrace, CancellationToken cancellationToken)
        {
            if (state?.OutputActions == null)
            {
                return;
            }

            await ProcessActionsAsync(lazyInput, context, state.OutputActions, stateTrace?.OutputActions, state, cancellationToken);
        }

        private async Task ProcessAfterStateChangedActionsAsync(State state, LazyInput lazyInput, IContext context, StateTrace stateTrace, CancellationToken cancellationToken)
        {
            if (state?.AfterStateChangedActions == null)
            {
                return;
            }

            await ProcessActionsAsync(lazyInput, context, state.AfterStateChangedActions, stateTrace?.AfterStateChangedActions, state, cancellationToken);
        }

        private async Task ProcessGlobalOutputActionsAsync(IContext context, Flow flow, LazyInput lazyInput, InputTrace inputTrace, State state, CancellationToken cancellationToken)
        {
            if (flow.OutputActions != null)
            {
                await ProcessActionsAsync(lazyInput, context, flow.OutputActions, inputTrace?.OutputActions, state, cancellationToken);
            }
        }

        private async Task ProcessGlobalAfterStateChangedActionsAsync(IContext context, Flow flow, LazyInput lazyInput, InputTrace inputTrace, State state, CancellationToken cancellationToken)
        {
            if (flow.AfterStateChangedActions != null)
            {
                await ProcessActionsAsync(lazyInput, context, flow.AfterStateChangedActions, inputTrace?.AfterStateChangedActions, state, cancellationToken);
            }
        }

        private async Task<(Flow, State, StateTrace, Stopwatch)> RedirectToSubflowAsync(IContext context, Identity userIdentity, State state, Flow parentFlow, StateTrace stateTrace, Stopwatch stateStopwatch, InputTrace inputTrace, LazyInput lazyInput, CancellationToken cancellationToken)
        {
            var shortNameOfSubflow = state.GetExtensionDataValue(SHORTNAME_OF_SUBFLOW_EXTENSION_DATA);
            if (shortNameOfSubflow.IsNullOrEmpty())
            {
                throw new ArgumentNullException($"Error on redirect to subflow '{state.Id}'");
            }

            // Create trace instances, if required
            var (newStateTrace, newStateStopwatch) = _traceManager.CreateStateTrace(inputTrace, state, stateTrace, stateStopwatch);

            await _stateManager.SetStateIdAsync(context, state.Id, cancellationToken);

            // Process the next state input actions
            await ProcessStateInputActionsAsync(state, lazyInput, context, stateTrace, cancellationToken);

            var subflow = await _flowLoader.LoadFlowAsync(FlowType.Subflow, parentFlow, shortNameOfSubflow, cancellationToken);
            if (subflow == null)
            {
                throw new ArgumentNullException($"Error on return subflow '{shortNameOfSubflow}'");
            }

            subflow.Validate();
            context.Flow = subflow;
            var newState = subflow.States.Single(s => s.Root);

            await _flowSessionManager.SetFlowSessionAsync(context, shortNameOfSubflow, cancellationToken);

            return (subflow, newState, newStateTrace, newStateStopwatch);
        }

        private async Task<(Flow, State, StateTrace, Stopwatch)> RedirectToParentFlowAsync(IContext context, Identity userIdentity, Flow flow, string previousStateId, string parentStateId, InputTrace inputTrace, LazyInput lazyInput, CancellationToken cancellationToken)
        {
            var parentFlow = flow.Parent;

            if (parentFlow == null)
            {
                throw new ArgumentNullException($"Error on return to parent flow of '{flow.Id}'");
            }

            await _stateManager.SetPreviousStateIdAsync(context, previousStateId, cancellationToken);
            await _stateManager.DeleteStateIdAsync(context, cancellationToken);

            context.Flow = parentFlow;
            var state = parentFlow.States.FirstOrDefault(s => s.Id == parentStateId) ?? parentFlow.States.Single(s => s.Root);

            await _flowSessionManager.SetFlowSessionAsync(context, parentFlow.SessionState, cancellationToken);

            // Create trace instances, if required
            var (stateTrace, stateStopwatch) = _traceManager.CreateStateTrace(inputTrace, state);

            // Prepare to leave the current state executing the output actions
            await ProcessStateOutputActionsAsync(state, lazyInput, context, stateTrace, cancellationToken);

            var previousState = state;
            state = await ProcessOutputsAsync(lazyInput, context, parentFlow, state, stateTrace?.Outputs, cancellationToken);

            // Only execute the ProcessAfterStateActionsAsync when the user current state changed after ProcessOutputsAsync
            if (previousState.Id != state.Id)
            {
                await ProcessAfterStateChangedActionsAsync(previousState, lazyInput, context, stateTrace, cancellationToken);
            }

            return (parentFlow, state, stateTrace, stateStopwatch);
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

        private async Task ProcessActionsAsync(LazyInput lazyInput, IContext context, Action[] actions, ICollection<ActionTrace> actionTraces, State state, CancellationToken cancellationToken)
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
                        JObject jObjectSettings = null;
                        var stringifySetting = stateAction.Settings?.ToString(Formatting.None);

                        if (stringifySetting != null)
                        {
                            if (action.Type != ACTION_EXECUTE_TEMPLATE)
                            {
                                stringifySetting = await _variableReplacer.ReplaceAsync(stringifySetting, context, cancellationToken, stateAction.Type);
                            }
                            jObjectSettings = JObject.Parse(stringifySetting);
                            AddStateIdToSettings(action.Type, jObjectSettings, state.Id);
                        }

                        if (actionTrace != null)
                        {
                            actionTrace.ParsedSettings = new JRaw(stringifySetting);
                        }

                        using (LogContext.PushProperty(nameof(BuilderException.MessageId), lazyInput?.Message?.Id))
                        using (LogContext.PushProperty(nameof(Action.Settings), jObjectSettings, true))
                            await action.ExecuteAsync(context, jObjectSettings, linkedCts.Token);
                    }
                    catch (Exception ex)
                    {
                        if (actionTrace != null)
                        {
                            if (!ex.Source.ToLower().StartsWith(START_SOURCE_TAKE_BLIP))
                            {
                                actionTrace.Error = STATE_TRACE_INTERNAL_SERVER_ERROR;
                            }
                            else
                            {
                                actionTrace.Error = ex.ToString();
                            }
                        }

                        var message = ex is OperationCanceledException && cts.IsCancellationRequested
                            ? $"The processing of the action '{stateAction.Type}' has timed out after {executionTimeout.TotalMilliseconds} ms"
                            : $"The processing of the action '{stateAction.Type}' has failed";

                        var actionProcessingException = new ActionProcessingException(message, ex)
                        {
                            ActionType = stateAction.Type,
                            ActionSettings = JsonConvert.DeserializeObject<IDictionary<string, object>>((string)stateAction.Settings)
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
                            if (!ex.Source.ToLower().StartsWith(START_SOURCE_TAKE_BLIP))
                            {
                                outputTrace.Error = STATE_TRACE_INTERNAL_SERVER_ERROR;
                            }
                            else
                            {
                                outputTrace.Error = ex.ToString();
                            }
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

        private async Task<bool> ValidateInputAsync(Message message, State state, LazyInput lazyInput, IContext context, CancellationTokenSource linkedCts)
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

                return false;
            }

            return true;
        }

        private void AddStateIdToSettings(string actionType, JObject jObjectSettings, string stateId)
        {
            if (actionType != ACTION_PROCESS_HTTP)
            {
                return;
            }

            jObjectSettings.Add(new JProperty("currentStateId", stateId));
        }

        private async Task<string> GetParentStateIdAsync(IContext context, Queue<string> parentStateIdQueue, CancellationToken cancellationToken) => parentStateIdQueue.Count > 0 ? parentStateIdQueue.Dequeue() : await _stateManager.GetParentStateIdAsync(context, cancellationToken);

        private async Task ClearInputExpirationCount(Message message)
        {
            if (!IsMessageFromExpiration(message))
            {
                return;
            }

            await _inputExpirationCount.TryRemoveAsync(message);
        }
        private bool IsMessageFromExpiration(Message message)
        {
            return message.Metadata?.ContainsKey(STATE_ID) ?? false;
        }
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