using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Diagnostics;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Defines a state in the conversation state machine.
    /// </summary>
    
    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public class State : IValidable
    {
        /// <summary>
        /// Unique id for the state. Required.
        /// </summary>
        [Required(ErrorMessage = "The state id is required")]
        public string Id { get; set; }
        
        /// <summary>
        /// Indicates if this is the root state if the user has no active conversation. Optional.
        /// </summary>
        public bool Root { get; set; }

        /// <summary>
        /// Determine the actions that should be executed when entering the state. Optional.
        /// </summary>
        public Action[] InputActions { get; set; }

        /// <summary>
        /// Indicates the input handling rules for the current state. Optional.
        /// </summary>
        public Input Input { get; set; }

        /// <summary>
        /// Determine the actions that should be executed before evaluate the state output. Optional.
        /// </summary>
        public Action[] OutputActions { get; set; }

        /// <summary>
        /// Determines the possible outputs and its conditions from the current state. Optional.
        /// Array of <see cref="Output"/>. Optional.
        /// </summary>
        public Output[] Outputs { get; set; }

        /// <summary>
        /// Stores additional JSON attributes that are not mapped in the type.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData { get; set; }

        public void Validate()
        {
            this.ValidateObject();

            if (InputActions != null)
            {
                foreach (var inputAction in InputActions)
                {
                    inputAction.Validate();
                }
            }

            Input?.Validate();

            if (OutputActions != null)
            {
                foreach (var outputAction in OutputActions)
                {
                    outputAction.Validate();
                }
            }

            if (Outputs != null)
            {
                foreach (var outputs in Outputs)
                {
                    outputs.Validate();
                }
            }
        }

        public StateTrace ToTrace()
        {
            return new StateTrace()
            {
                Id = Id,
                ExtensionData = ExtensionData
            };
        }
    }
}
