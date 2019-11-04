using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Defines a conversational state machine.
    /// </summary>
    public class Flow : IValidable
    {
        private bool _isValid;
        private BuilderConfiguration _builderConfiguration;
        
        /// <summary>
        /// The unique identifier of the flow. Required.
        /// </summary>
        [Required(ErrorMessage = "The flow id is required")]
        public string Id { get; set; }
        
        /// <summary>
        /// Determine the global actions that should be executed before processing the input. Optional.
        /// </summary>
        public Action[] InputActions { get; set; }
        
        /// <summary>
        /// The flow states. Required.
        /// </summary>
        [Required(ErrorMessage = "The flow must have at least one state")]
        public State[] States { get; set; }

        /// <summary>
        /// Determine the global actions that should be executed after processing the input. Optional.
        /// </summary>
        public Action[] OutputActions { get; set; }

        /// <summary>
        /// The flow configuration.
        /// </summary>
        public Dictionary<string, string> Configuration { get; set; }

        /// <summary>
        /// Provides a view over the 'builder:' <see cref="Configuration"/> keys.
        /// </summary>
        [JsonIgnore]
        public BuilderConfiguration BuilderConfiguration
        {
            get
            {
                if (_builderConfiguration == null)
                {
                    if (Configuration != null)
                    {
                        _builderConfiguration = BuilderConfiguration.FromDictionary(Configuration);
                    }
                    else
                    {
                        _builderConfiguration = new BuilderConfiguration();
                    }
                }

                return _builderConfiguration;
            }
        }

        /// <summary>
        /// The flow trace settings
        /// </summary>
        public TraceSettings TraceSettings { get; set; }

        public void Validate()
        {
            // Optimization to avoid multiple validations.
            // It can lead to errors if any property is changed meanwhile...
            if (_isValid) return;

            this.ValidateObject();

            if (States.Count(s => s.Root) != 1)
            {
                throw new ValidationException("The flow must have one root state");
            }

            var rootState = States.First(s => s.Root);
            if (rootState.Input == null || rootState.Input.Bypass)
            {
                throw new ValidationException("The root state must expect an input");
            }

            if (rootState.Input.Conditions?.Any() == true)
            {
                throw new ValidationException("The root state must not have any conditions");
            }
            
            
            if (InputActions != null)
            {
                foreach (var inputAction in InputActions)
                {
                    inputAction.Validate();
                }
            }

            foreach (var state in States)
            {
                state.Validate();

                if (States.Count(s => s.Id == state.Id) > 1)
                {
                    throw new ValidationException($"The state id '{state.Id}' is not unique in the flow");
                }

                // Check if there's a direct path loop (without inputs) to this state in the flow.
                if (state.Outputs != null)
                {
                    bool CanBeReached(State targetState, Output output, ISet<string> checkedStates)
                    {
                        if (checkedStates.Contains(output.StateId)) return false;
                        var outputState = States.FirstOrDefault(s => s.Id == output.StateId);
                        if (outputState?.Outputs == null || outputState.Outputs.Length == 0) return false;
                        if (outputState.Input != null && !outputState.Input.Bypass) return false;
                        if (outputState.Outputs.Any(o => o.StateId == targetState.Id)) return true;
                        checkedStates.Add(output.StateId);
                        return outputState.Outputs.Any(o => CanBeReached(targetState, o, checkedStates));
                    }

                    ;

                    foreach (var output in state.Outputs)
                    {
                        if (States.All(s => s.Id != output.StateId))
                        {
                            throw new ValidationException($"The output state id '{output.StateId}' is invalid");
                        }

                        if (state.Input == null || state.Input.Bypass)
                        {
                            var checkedStates = new HashSet<string>();

                            if (CanBeReached(state, output, checkedStates))
                            {
                                throw new ValidationException(
                                    $"There is a loop in the flow starting in the state {state.Id} that does not requires user input");
                            }
                        }
                    }
                }
            }
            
            if (OutputActions != null)
            {
                foreach (var outputAction in OutputActions)
                {
                    outputAction.Validate();
                }
            }

            // Try create trace settings from configuration keys
            if (TraceSettings == null &&
                Configuration != null &&
                Configuration.TryGetValue("TraceMode", out var traceModeValue) &&
                Enum.TryParse<TraceMode>(traceModeValue, true, out var traceMode) &&
                Configuration.TryGetValue("TraceTargetType", out var traceTargetTypeValue) &&
                Enum.TryParse<TraceTargetType>(traceTargetTypeValue, true, out var traceTargetType) &&
                Configuration.TryGetValue("TraceTarget", out var traceTarget))
            {
                TraceSettings = new TraceSettings
                {
                    Mode = traceMode,
                    TargetType = traceTargetType,
                    Target = traceTarget
                };

                if (Configuration.TryGetValue("TraceSlowThreshold", out var traceSlowThresholdValue) &&
                    int.TryParse(traceSlowThresholdValue, out var traceSlowThreshold))
                {
                    TraceSettings.SlowThreshold = traceSlowThreshold;
                }
            }


            _isValid = true;
        }

        /// <summary>
        /// Creates an instance of <see cref="Flow"/> from a JSON input.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static Flow ParseFromJson(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            return JsonConvert.DeserializeObject<Flow>(json, JsonSerializerSettingsContainer.Settings);
        }

        /// <summary>
        /// Creates an instance of <see cref="Flow" /> from a JSON file.
        /// </summary>
        /// <param name="filePath">The path.</param>
        /// <returns></returns>
        public static Flow ParseFromJsonFile(string filePath) => ParseFromJson(File.ReadAllText(filePath));

        /// <summary>
        /// Fully checks if a given config header should be added or not
        /// </summary>
        /// <param name="header">Config header to check for</param>
        /// <returns>Boolean indicating if the configuration is enabled</returns>
        public bool IsConfigurationFlagEnabled(string header)
        {
            return Configuration != null &&
                Configuration.TryGetValue(header, out string identifierHeaderValue) &&
                bool.TryParse(identifierHeaderValue, out bool sendBotIdentifier) &&
                sendBotIdentifier;
        }
    }
}