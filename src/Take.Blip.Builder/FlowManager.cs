using Blip.Ai.Bot.Monitoring.Logging.Enums;
using Blip.Ai.Bot.Monitoring.Logging.Interface;
using Blip.Ai.Bot.Monitoring.Logging.Models;
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
using Take.Blip.Builder.Actions.ExecuteTemplate;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Builder;
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
        private readonly IBuilderExtension _builderExtension;
        private readonly IBlipLogger _blipMonitoringLogger;
        private readonly IVariableReplacer _variableReplacer;
        private readonly ILogger _logger;
        private readonly ITraceManager _traceManager;
        private readonly IUserOwnerResolver _userOwnerResolver;
        private readonly Node _applicationNode;
        private readonly IAnalyzeBuilderExceptions _analyzeBuilderExceptions;
        private readonly IInputMessageHandler _inputMessageHandlerAggregator;
        private readonly IInputExpirationCount _inputExpirationCount;
        private readonly IBlipLogger _BlipMonitoringLogger;

        private const string SHORTNAME_OF_SUBFLOW_EXTENSION_DATA = "shortNameOfSubflow";
        private const string ACTION_PROCESS_HTTP = "ProcessHttp";
        private const string ACTION_EXECUTE_TEMPLATE = "ExecuteTemplate";
        public const string STATE_ID = "inputExpiration.stateId";
        private const string START_SOURCE_TAKE_BLIP = "take.blip";
        private const string STATE_TRACE_INTERNAL_SERVER_ERROR = "Internal Server Error";
        private const string ACTION_BLIP_FUNCTION = "ExecuteBlipFunction";
        private const string ACTION_EXECUTE_SCRIPT_V2 = "ExecuteScriptV2";
        private const string WORD_START_BLIP_FUNCTION = "{{";
        private const string TYPE_ACTION_INPUT = "inputActions";
        private const string TYPE_ACTION_OUTPUT = "outputActions";

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
            IInputExpirationCount inputExpirationCount,
            IBuilderExtension builderExtension,
            IBlipLogger blipMonitoringLogger
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
            _builderExtension = builderExtension;
            _blipMonitoringLogger = blipMonitoringLogger;
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

                inputTrace = new InputTrace
                {
                    Owner = ownerIdentity,
                    FlowId = flow.Id,
                    User = userIdentity,
                    Input = message.Content.ToString()
                };


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
                            await ProcessActionsAsync(lazyInput, context, flow.InputActions, inputTrace?.InputActions, state,TYPE_ACTION_INPUT, linkedCts.Token);
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
                    if (traceSettings != null && traceSettings.Mode != TraceMode.Disabled)
                    {
                        await _traceManager.ProcessTraceAsync(inputTrace, traceSettings, inputStopwatch, cts.Token);
                    }
                    _logger.Warning("Artur input trace SDK: {0}", System.Text.Json.JsonSerializer.Serialize(inputTrace));

                    _blipMonitoringLogger.ActionExecution(
                        new LogInput
                        {
                            Data = new JObject
                            {
                                ["flowId"] = flow.Id,
                                ["stateId"] = state?.Id,
                                ["input"] = message.Content.ToString(),
                                ["inputExecutionTime"] = inputStopwatch?.ElapsedMilliseconds ?? 0,
                                ["error"] = inputTrace?.Error,
                                ["inputTrace"] = inputTrace != null ? System.Text.Json.JsonSerializer.Serialize(inputTrace) : null,
                                ["traceSettings"] = traceSettings != null ? System.Text.Json.JsonSerializer.Serialize(traceSettings) : null,
                            },
                            IdMessage = message.Id,
                            From = userIdentity,
                            To = ownerIdentity,
                            Title = "Input Processing"
                        });
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

            await ProcessActionsAsync(lazyInput, context, state.InputActions, stateTrace?.InputActions, state, TYPE_ACTION_INPUT, cancellationToken);
        }

        private async Task ProcessStateOutputActionsAsync(State state, LazyInput lazyInput, IContext context, StateTrace stateTrace, CancellationToken cancellationToken)
        {
            if (state?.OutputActions == null)
            {
                return;
            }

            await ProcessActionsAsync(lazyInput, context, state.OutputActions, stateTrace?.OutputActions, state,TYPE_ACTION_OUTPUT, cancellationToken);
        }

        private async Task ProcessAfterStateChangedActionsAsync(State state, LazyInput lazyInput, IContext context, StateTrace stateTrace, CancellationToken cancellationToken)
        {
            if (state?.AfterStateChangedActions == null)
            {
                return;
            }

            await ProcessActionsAsync(lazyInput, context, state.AfterStateChangedActions, stateTrace?.AfterStateChangedActions, state,TYPE_ACTION_INPUT, cancellationToken);
        }

        private async Task ProcessGlobalOutputActionsAsync(IContext context, Flow flow, LazyInput lazyInput, InputTrace inputTrace, State state, CancellationToken cancellationToken)
        {
            if (flow.OutputActions != null)
            {
                await ProcessActionsAsync(lazyInput, context, flow.OutputActions, inputTrace?.OutputActions, state, TYPE_ACTION_OUTPUT,cancellationToken);
            }
        }

        private async Task ProcessGlobalAfterStateChangedActionsAsync(IContext context, Flow flow, LazyInput lazyInput, InputTrace inputTrace, State state, CancellationToken cancellationToken)
        {
            if (flow.AfterStateChangedActions != null)
            {
                await ProcessActionsAsync(lazyInput, context, flow.AfterStateChangedActions, inputTrace?.AfterStateChangedActions, state, TYPE_ACTION_INPUT, cancellationToken);
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

        private async Task ProcessActionsAsync(LazyInput lazyInput, IContext context, Action[] actions, ICollection<ActionTrace> actionTraces, State state, string typeAction, CancellationToken cancellationToken)
        {
            // Execute all state actions
            foreach (var stateAction in actions.OrderBy(a => a.Order))
            {
                if (stateAction.Conditions != null && lazyInput != null &&
                    !await stateAction.Conditions.EvaluateConditionsAsync(lazyInput, context, cancellationToken))
                {
                    continue;
                }

                string realAction = stateAction.Type;

                if (stateAction.Type == ACTION_BLIP_FUNCTION)
                {
                    stateAction.Type = ACTION_EXECUTE_SCRIPT_V2;
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
                        var stringfySettingsCopy = stringifySetting;

                        if (stringifySetting != null)
                        {
                            if (action.Type != ACTION_EXECUTE_TEMPLATE)
                            {
                                stringifySetting = await _variableReplacer.ReplaceAsync(stringifySetting, context, cancellationToken, stateAction.Type);
                            }
                            jObjectSettings = JObject.Parse(stringifySetting);
                            if (realAction == ACTION_BLIP_FUNCTION)
                            {
                                var functionOnBlipFunction = await _builderExtension.GetFunctionOnBlipFunctionAsync(jObjectSettings["source"].ToString(), linkedCts.Token);
                                var function = functionOnBlipFunction.ToObject<Function>();
                                jObjectSettings["source"] = function.FunctionContent;
                                if (function.FunctionContent.StartsWith(WORD_START_BLIP_FUNCTION))
                                {
                                    var stringJobectSettings = JsonConvert.SerializeObject(jObjectSettings);
                                    var newStringifySetting = await _variableReplacer.ReplaceAsync(stringJobectSettings, context, cancellationToken, stateAction.Type);
                                    jObjectSettings = JObject.Parse(newStringifySetting);
                                }
                            }
                            AddStateIdToSettings(action.Type, jObjectSettings, state.Id);
                        }

                        if (actionTrace != null)
                        {
                            if (realAction == ACTION_PROCESS_HTTP)
                            {
                                var result = RestoreBodyStringWithSecrets(stringfySettingsCopy, stringifySetting);
                                actionTrace.ParsedSettings = new JRaw(string.IsNullOrEmpty(result) ? stringifySetting : result);
                                jObjectSettings = JObject.Parse(result);
                            }
                            else
                            {
                                actionTrace.ParsedSettings = new JRaw(stringifySetting);

                            }
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
                        if (realAction == ACTION_BLIP_FUNCTION)
                        {
                            stateAction.Type = ACTION_BLIP_FUNCTION;
                        }

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

        private string RestoreBodyStringWithSecrets(string original, string executed)
        {
            var regexMatch = @"{{secret\.([a-zA-Z0-9_]+)}}";

            var originalObj = JObject.Parse(original);
            var executedObj = JObject.Parse(executed);

            var originalBodyRaw = originalObj["body"]?.ToString();
            var executedBodyRaw = executedObj["body"]?.ToString();

            if (string.IsNullOrWhiteSpace(originalBodyRaw) || string.IsNullOrWhiteSpace(executedBodyRaw))
                return executed;

            try
            {
                var originalBodyToken = TryParseJson(originalBodyRaw);
                var executedBodyToken = TryParseJson(executedBodyRaw);

                if (originalBodyToken != null && executedBodyToken != null)
                {
                    var originalLeaves = originalBodyToken.SelectTokens("$..*").ToList();
                    var executedLeaves = executedBodyToken.SelectTokens("$..*").ToList();

                    for (int i = 0; i < originalLeaves.Count && i < executedLeaves.Count; i++)
                    {
                        var origVal = originalLeaves[i];
                        if (origVal.Type == JTokenType.String)
                        {
                            var val = origVal.ToString();
                            if (Regex.IsMatch(val, $"^({regexMatch})$"))
                            {
                                executedLeaves[i].Replace(val);
                            }
                        }
                    }

                    executedObj["body"] = executedBodyToken.ToString(Formatting.None);
                }
                else
                {
                    executedObj["body"] = RestoreStringSecrets(originalBodyRaw, executedBodyRaw, regexMatch);
                }
            }
            catch
            {
                executedObj["body"] = RestoreStringSecrets(originalBodyRaw, executedBodyRaw, regexMatch);
            }

            var originalUri = originalObj["uri"]?.ToString();
            if (Regex.IsMatch(originalUri ?? "", regexMatch))
            {
                executedObj["SecretUrlBlip"] = true;
            }

            return executedObj.ToString(Formatting.Indented);
        }

        private static JToken? TryParseJson(string input)
        {
            try
            {
                return JToken.Parse(input);
            }
            catch
            {
                return null;
            }
        }

        private static string RestoreStringSecrets(string original, string executed, string regexPattern)
        {
            if (original.Contains("=") && original.Contains("&"))
            {
                var originalParams = ParseFormEncoded(original);
                var executedParams = ParseFormEncoded(executed);

                foreach (var key in originalParams.Keys)
                {
                    var origVal = originalParams[key];
                    if (Regex.IsMatch(origVal, $"^({regexPattern})$"))
                    {
                        executedParams[key] = origVal;
                    }
                }

                return string.Join("&", executedParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            }
            else
            {
                var matches = Regex.Matches(original, regexPattern);
                foreach (Match match in matches)
                {
                    var placeholder = match.Value;
                    if (executed.Contains(placeholder))
                        continue;

                    var base64LikeMatch = Regex.Match(executed, @"[a-zA-Z0-9+/=]{10,}");
                    if (base64LikeMatch.Success)
                    {
                        executed = executed.Replace(base64LikeMatch.Value, placeholder);
                    }
                }
                return executed;
            }
        }

        private static Dictionary<string, string> ParseFormEncoded(string formEncoded)
        {
            if (string.IsNullOrEmpty(formEncoded))
            {
                return new Dictionary<string, string>();
            }

            return formEncoded
                .Split('&')
                .Where(pairString => !string.IsNullOrWhiteSpace(pairString))
                .Select(pairString =>
                {
                    string[] keyValue = pairString.Split(new[] { '=' }, 2);
                    var key = Uri.UnescapeDataString(keyValue[0]);
                    var value = keyValue.Length == 2 ? Uri.UnescapeDataString(keyValue[1]) : string.Empty;
                    return new { Key = key, Value = value };
                })
                .ToDictionary(pair => pair.Key, pair => pair.Value);
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
                // Evaluate each output conditions
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

                                throw new InvalidOperationException($"Failed to process output condition, because the output context variable '{output.StateId}' is undefined or does not exist in the context.");
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

                        _blipMonitoringLogger.UserInput(new LogInput
                        {
                            Data = new JObject
                            {
                                ["outputStateId"] = output.StateId,
                                ["outputConditions"] = output.Conditions != null ? JToken.FromObject(output.Conditions) : null,
                                ["conditionsCount"] = output.Conditions?.Count() ?? 0,
                                ["outputOrder"] = output.Order,
                                ["outputTrace"] = JToken.FromObject(output)
                            },
                            IdMessage = lazyInput.Message.Id,
                            From = context.UserIdentity,
                            To = context.OwnerIdentity,
                            Title = "Output Processing"
                        });


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


        #region Builder Agent Methods

        /// <summary>
        /// Processes a command input for a specific local custom action in a flow.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="flow"></param>
        /// <param name="stateId"></param>
        /// <param name="actionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A dictionary with variable names and values given a local custom action</returns>
        public async Task<Dictionary<string, string>> ProcessCommandInputAsync(Message message, Flow flow, string stateId, string actionId, CancellationToken cancellationToken)
        {
            flow.Validate();

            // Determine the user / owner pair
            // on new action command we need to create a command similar to the message to identity properly
            var (userIdentity, ownerIdentity) = await _userOwnerResolver.GetUserOwnerIdentitiesAsync(message, flow.BuilderConfiguration, cancellationToken);

            // Input tracing infrastructure
            InputTrace inputTrace = null;

            var traceSettings = message.Metadata != null && message.Metadata.Keys.Contains(TraceSettings.BUILDER_TRACE_TARGET_TYPE)
                ? new TraceSettings(message.Metadata)
                : flow.TraceSettings;

            if (traceSettings != null && traceSettings.Mode != TraceMode.Disabled)
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

            // Allow execution be under the properly ownerIdentity
            // This is important to make the contexts be stored on router context instead of subbot
            var ownerContext = OwnerContext.Create(ownerIdentity);

            State state = default;

            try
            {
                using (var cts = new CancellationTokenSource(_configuration.InputProcessingTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                {
                    // Synchronize to avoid concurrency issues on multiple running instances
                    // Creating this semaphore to avoid to instances executing the same local custom action and avoid context dirty reads
                    var handle = await _flowSemaphore.WaitAsync(flow, actionId, userIdentity, _configuration.DefaultActionExecutionTimeout, cancellationToken);

                    try
                    {
                        // Create the input evaluator
                        var lazyInput = new LazyInput(message, userIdentity, flow.BuilderConfiguration, _documentSerializer,
                            _envelopeSerializer, _artificialIntelligenceExtension, linkedCts.Token);

                        // Load the user context
                        var context = _contextProvider.CreateContext(userIdentity, ownerIdentity, lazyInput, flow);

                        // Validate if the user are in the informed state
                        var currentState = await _stateManager.GetStateIdAsync(context, linkedCts.Token);

                        if (currentState is null || !currentState.Equals(stateId, StringComparison.InvariantCultureIgnoreCase))
                            throw new BuilderException("user not in the informed state");

                        // Get the state object based on received state id
                        state = flow.States.FirstOrDefault(s => s.Id == stateId) ?? flow.States.Single(s => s.Root);

                        // Create trace instances, if required
                        var (stateTrace, stateStopwatch) = _traceManager.CreateStateTrace(inputTrace, state);

                        // Process the Local Custom Action
                        var outputVariablesProperties = await ProcessStateLocalCustomActionAsync(state, context, stateTrace, actionId, linkedCts.Token);

                        // In case of desired action doesn't have values to be returned to caller
                        if (outputVariablesProperties == null)
                            return null;

                        var outputVariables = new Dictionary<string, string>();

                        // Create the dictionary based on user context to return to caller
                        outputVariables = await GetContextVariablesFromActionExecutionAsync(outputVariablesProperties, context, linkedCts.Token);

                        return outputVariables;
                    }
                    finally
                    {
                        // Dispose the semaphore handle to allow other instances to execute the same local custom action, if necessary
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
                    new BuilderException($"Error processing single action input with custom message id '{message.Id}' for user '{userIdentity}' in state '{stateId}'", ex);

                builderException.StateId = stateId;
                builderException.UserId = userIdentity;

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

        /// <summary>
        /// Processes a local custom action for a given state, returning the output variables names properties.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="lazyInput"></param>
        /// <param name="context"></param>
        /// <param name="stateTrace"></param>
        /// <param name="actionId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> ProcessStateLocalCustomActionAsync(State state, IContext context, StateTrace stateTrace, string actionId, CancellationToken cancellationToken)
        {
            // Validating if the state has local custom actions to be executed
            if (state?.LocalCustomActions == null)
                return null;

            // Getting the properly action based on the actionId informed on the execution
            var actionToExecute = state.LocalCustomActions.FirstOrDefault(action => action.Id.Equals(actionId, StringComparison.InvariantCultureIgnoreCase));

            if (actionToExecute == null)
                return null;

            // Getting the output variables list to be retrieved from user context
            var outputVariablesProperties = await ProcessSingleActionAsync(context, actionToExecute, stateTrace?.LocalCustomActions, state, cancellationToken);

            if (outputVariablesProperties == null || outputVariablesProperties.Length == 0)
                return null;

            // Deserializing the action settings to allow the retrieval of the output variables names properties
            var actionInformations = DeserializeActionSettings(actionToExecute.Settings);

            // Search in the dictionary what output variables we have configured on executed action
            var variableNames = outputVariablesProperties
                .Where(variable => actionInformations?.ContainsKey(variable) == true &&
                                  !string.IsNullOrWhiteSpace(actionInformations[variable].ToString()))
                .Select(name => actionInformations[name].ToString());

            return variableNames;
        }

        /// <summary>
        /// Processes a single action in the context of a state, returning the output variables names properties.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stateAction"></param>
        /// <param name="actionTraces"></param>
        /// <param name="state"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string[]> ProcessSingleActionAsync(IContext context, Action stateAction, ICollection<ActionTrace> actionTraces, State state, CancellationToken cancellationToken)
        {
            // Different from message, local custom actions will not evaluate the action conditions to execute

            string realAction = stateAction.Type;

            if (stateAction.Type == ACTION_BLIP_FUNCTION)
                stateAction.Type = ACTION_EXECUTE_SCRIPT_V2;

            // Get action from dependency injection to execute
            var action = _actionProvider.Get(stateAction.Type);

            // Trace infra
            var (actionTrace, actionStopwatch) = actionTraces != null
                ? (stateAction.ToTrace(), Stopwatch.StartNew())
                : (null, null);

            if (actionTrace != null)
                context.SetCurrentActionTrace(actionTrace);

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
                    // Copy the settings to a JObject to be used by the action to allow remove sensible content
                    JObject jObjectSettings = null;
                    var stringifySetting = stateAction.Settings?.ToString(Formatting.None);
                    var stringfySettingsCopy = stringifySetting;

                    if (stringifySetting != null)
                    {
                        if (action.Type != ACTION_EXECUTE_TEMPLATE)
                        {
                            stringifySetting = await _variableReplacer.ReplaceAsync(stringifySetting, context, cancellationToken, stateAction.Type);
                        }

                        jObjectSettings = JObject.Parse(stringifySetting);

                        if (realAction == ACTION_BLIP_FUNCTION)
                        {
                            var functionOnBlipFunction = await _builderExtension.GetFunctionOnBlipFunctionAsync(jObjectSettings["source"].ToString(), linkedCts.Token);
                            var function = functionOnBlipFunction.ToObject<Function>();
                            jObjectSettings["source"] = function.FunctionContent;
                            if (function.FunctionContent.StartsWith(WORD_START_BLIP_FUNCTION))
                            {
                                var stringJobectSettings = JsonConvert.SerializeObject(jObjectSettings);
                                var newStringifySetting = await _variableReplacer.ReplaceAsync(stringJobectSettings, context, cancellationToken, stateAction.Type);
                                jObjectSettings = JObject.Parse(newStringifySetting);
                            }
                        }

                        AddStateIdToSettings(action.Type, jObjectSettings, state.Id);
                    }

                    if (actionTrace != null)
                    {
                        if (realAction == ACTION_PROCESS_HTTP)
                        {
                            var result = RestoreBodyStringWithSecrets(stringfySettingsCopy, stringifySetting);
                            actionTrace.ParsedSettings = new JRaw(string.IsNullOrEmpty(result) ? stringifySetting : result);
                            jObjectSettings = JObject.Parse(result);
                        }
                        else
                        {
                            actionTrace.ParsedSettings = new JRaw(stringifySetting);

                        }
                    }

                    using (LogContext.PushProperty(nameof(Action.Settings), jObjectSettings, true))
                        await action.ExecuteAsync(context, jObjectSettings, linkedCts.Token);

                    // Return the output variables names properties from the executed action
                    return action.OutputVariables;
                }
                catch (Exception ex)
                {
                    if (actionTrace != null)
                        actionTrace.Error = !ex.Source.ToLower().StartsWith(START_SOURCE_TAKE_BLIP)
                            ? STATE_TRACE_INTERNAL_SERVER_ERROR
                            : ex.ToString();

                    var message = ex is OperationCanceledException && cts.IsCancellationRequested
                        ? $"The processing of the single action '{stateAction.Type}' has timed out after {executionTimeout.TotalMilliseconds} ms"
                        : $"The processing of the single action '{stateAction.Type}' has failed";

                    var actionProcessingException = new ActionProcessingException(message, ex)
                    {
                        ActionType = stateAction.Type,
                        ActionSettings = JsonConvert.DeserializeObject<IDictionary<string, object>>((string)stateAction.Settings)
                    };

                    if (stateAction.ContinueOnError)
                    {
                        _logger.Warning(actionProcessingException, "Action '{ActionType}' has failed but was forgotten", stateAction.Type);
                        return null;
                    }
                    else
                        throw actionProcessingException;

                }
                finally
                {
                    if (realAction == ACTION_BLIP_FUNCTION)
                        stateAction.Type = ACTION_BLIP_FUNCTION;

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

        /// <summary>
        /// Retrieves the context variables from the action execution.
        /// </summary>
        /// <param name="outputVariablesProperties"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> GetContextVariablesFromActionExecutionAsync(IEnumerable<string> outputVariablesProperties, IContext context, CancellationToken cancellationToken)
        {
            var tasks = outputVariablesProperties
                .Select(async variableName =>
                                    new
                                    {
                                        Key = variableName,
                                        Value = await context.GetContextVariableAsync(variableName, cancellationToken)
                                    });

            var results = await Task.WhenAll(tasks);

            return results.ToDictionary(result => result.Key, result => result.Value);
        }

        /// <summary>
        /// Deserializes the action settings from a JToken to a dictionary. The object value occurs due some types with custom types, like ProcessHttp headers
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        private IDictionary<string, object> DeserializeActionSettings(JToken settings, string actionId = null)
        {
            if (settings == null)
                return new Dictionary<string, object>();

            try
            {
                // Handle both JObject and string cases
                var settingsObject = settings as JObject ?? JObject.Parse(settings.ToString());
                return settingsObject.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
            }
            catch (JsonException ex)
            {
                _logger.Warning(ex, "Failed to deserialize action settings for action {ActionId}. Settings: {Settings}",
                               actionId, settings.ToString());
                return new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error deserializing action settings for action {ActionId}", actionId);
                return new Dictionary<string, object>();
            }
        }

        #endregion

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