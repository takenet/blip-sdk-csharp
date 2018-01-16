using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;
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
        private readonly IArtificialIntelligenceExtension _artificialIntelligenceExtension;
        private readonly IVariableReplacer _variableReplacer;

        public FlowManager(
            IConfiguration configuration,
            IStateManager stateManager, 
            IContextProvider contextProvider, 
            INamedSemaphore namedSemaphore, 
            IActionProvider actionProvider,
            ISender sender,
            IDocumentSerializer documentSerializer,
            IArtificialIntelligenceExtension artificialIntelligenceExtension,
            IVariableReplacer variableReplacer)
        {
            _configuration = configuration;
            _stateManager = stateManager;
            _contextProvider = contextProvider;
            _namedSemaphore = namedSemaphore;
            _actionProvider = actionProvider;
            _sender = sender;
            _documentSerializer = documentSerializer;
            _artificialIntelligenceExtension = artificialIntelligenceExtension;
            _variableReplacer = variableReplacer;
        }

        public async Task ProcessInputAsync(Document input, Identity user, Flow flow, CancellationToken cancellationToken)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (flow == null) throw new ArgumentNullException(nameof(flow));
            flow.Validate();

            // Create the input evaluator
            var lazyInput = new LazyInput(input, _documentSerializer, _artificialIntelligenceExtension, cancellationToken);

            // Synchronize to avoid concurrency issues on multiple running instances
            var handle = await _namedSemaphore.WaitAsync($"{flow.Id}:{user}", _configuration.ExecutionSemaphoreExpiration, cancellationToken);
            try
            {
                // Try restore a stored state
                var stateId = await _stateManager.GetStateIdAsync(flow.Id, user, cancellationToken);
                var state = flow.States.FirstOrDefault(s => s.Id == stateId) ?? flow.States.Single(s => s.Root);

                // Load the user context
                var context = _contextProvider.GetContext(user, flow.Id);

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Validate the input for the current state
                    if (state.Input != null 
                        && !state.Input.Bypass 
                        && state.Input.Validation != null 
                        && !ValidateDocument(lazyInput, state.Input.Validation))
                    {
                        if (state.Input.Validation.Error != null)
                        {
                            // Send the validation error message
                            await _sender.SendMessageAsync(state.Input.Validation.Error, user.ToNode(),
                                cancellationToken);
                        }
                        break;
                    }

                    // Set the input in the context
                    if (!string.IsNullOrEmpty(state.Input?.Variable))
                    {
                        await context.SetVariableAsync(state.Input.Variable, lazyInput.SerializedInput, cancellationToken);
                    }

                    // Prepare to leave the current state executing the output actions
                    if (state.OutputActions != null)
                    {
                        await ProcessActionsAsync(context, state.OutputActions, cancellationToken);
                    }

                    // Determine the next state
                    state = await ProcessOutputsAsync(lazyInput, context, flow, state, cancellationToken);

                    // Store the next state
                    if (state != null)
                    {
                        await _stateManager.SetStateIdAsync(flow.Id, context.User, state.Id, cancellationToken);
                    }
                    else
                    {
                        await _stateManager.DeleteStateIdAsync(flow.Id, context.User, cancellationToken);
                    }

                    // Process the next state input actions
                    if (state?.InputActions != null)
                    {
                        await ProcessActionsAsync(context, state.InputActions, cancellationToken);
                    }

                    // Continue processing if the next has do not expect the user input
                } while (state != null && (state.Input == null || state.Input.Bypass));
            }
            finally
            {
                await handle.DisposeAsync();
            }
        }

        private bool ValidateDocument(LazyInput lazyInput, InputValidation inputValidation)
        {
            switch (inputValidation.Rule)
            {
                case InputValidationRule.Text:
                    return lazyInput.Input is PlainText;

                case InputValidationRule.Number:
                    return decimal.TryParse(lazyInput.SerializedInput, out _);

                case InputValidationRule.Date:
                    return DateTime.TryParse(lazyInput.SerializedInput, out _);

                case InputValidationRule.Regex:
                    return Regex.IsMatch(lazyInput.SerializedInput, inputValidation.Regex);

                case InputValidationRule.Type:
                    return lazyInput.Input.GetMediaType() == inputValidation.Type;

                default:
                    throw new ArgumentOutOfRangeException(nameof(inputValidation));
            }
        }

        private async Task ProcessActionsAsync(IContext context, Action[] actions, CancellationToken cancellationToken)
        {
            // Execute all state actions
            foreach (var stateAction in actions.OrderBy(a => a.Order))
            {                
                var action = _actionProvider.Get(stateAction.Type);

                var settings = stateAction.Settings;
                if (settings != null)
                {
                    var settingsJson = settings.ToString(Formatting.None);
                    settingsJson = await _variableReplacer.ReplaceAsync(settingsJson, context, cancellationToken);
                    settings = JObject.Parse(settingsJson);
                }

                await action.ExecuteAsync(context, settings, cancellationToken);
            }
        }

        private async Task<State> ProcessOutputsAsync(LazyInput lazyInput, IContext context, Flow flow, State state, CancellationToken cancellationToken)
        {
            var outputs = state.Outputs;
            state = null;

            // If there's any output in the current state
            if (outputs != null)
            {
                // Evalute each output conditions
                foreach (var output in outputs.OrderBy(o => o.Order))
                {
                    var isValidOutput = true;

                    if (output.Conditions != null)
                    {
                        foreach (var outputCondition in output.Conditions)
                        {
                            isValidOutput = await EvaluateConditionAsync(outputCondition, lazyInput, context, cancellationToken);
                            if (!isValidOutput) break;
                        }
                    }

                    if (isValidOutput)
                    {
                        state = flow.States.SingleOrDefault(s => s.Id == output.StateId);
                        break;
                    }
                }
            }

            return state;
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
                    comparisonValue = lazyInput.SerializedInput;
                    break;

                case ValueSource.Context:
                    comparisonValue = await context.GetVariableAsync(condition.Variable, cancellationToken);
                    break;

                case ValueSource.Intent:
                    comparisonValue = (await lazyInput.AnalyzedInput)
                        .Intentions?
                        .OrderByDescending(i => i.Score)
                        .FirstOrDefault(i => i.Score >= 0.5)?
                        .Name;
                    break;

                case ValueSource.Entity:
                    comparisonValue = (await lazyInput.AnalyzedInput)
                        .Entities?
                        .FirstOrDefault(e => e.Name != null && e.Name.Equals(condition.Entity, StringComparison.OrdinalIgnoreCase))?
                        .Value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (comparisonValue == null) return false;

            var comparisonFunc = condition.Comparison.ToDelegate();

            switch (condition.Operator)
            {
                case ConditionOperator.Or:
                    return condition.Values.Any(v => comparisonFunc(comparisonValue, v));

                case ConditionOperator.And:
                    return condition.Values.All(v => comparisonFunc(comparisonValue, v));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Allows the lazy evaluation of input bound values.
        /// This optimizes the calls for serialization and the AI extension.
        /// </summary>
        private class LazyInput
        {
            private readonly Lazy<string> _inputSource;
            private readonly Lazy<Task<AnalysisResponse>> _analysisSource;

            public LazyInput(
                Document input, 
                IDocumentSerializer documentSerializer, 
                IArtificialIntelligenceExtension artificialIntelligenceExtension,
                CancellationToken cancellationToken)
            {
                Input = input;
                _inputSource = new Lazy<string>(() => documentSerializer.Serialize(input));
                _analysisSource = new Lazy<Task<AnalysisResponse>>(() => artificialIntelligenceExtension.AnalyzeAsync(
                    new AnalysisRequest
                    {
                        Text = _inputSource.Value
                    },
                    cancellationToken));
            }

            public Document Input { get; }

            public string SerializedInput => _inputSource.Value;

            public Task<AnalysisResponse> AnalyzedInput => _analysisSource.Value;
        }
    }
}