using System.ComponentModel.DataAnnotations;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Defines a state in the conversation state machine.
    /// </summary>
    public class State
    {
        /// <summary>
        /// Unique id for the state. Required.
        /// </summary>
        [Required]
        public string Id { get; set; }
        
        /// <summary>
        /// Indicates if this is the root state if the user has no active conversation. Optional.
        /// </summary>
        public bool Root { get; set; }

        /// <summary>
        /// Determine the actions that should be executed before awaiting for the user input. Optional.
        /// </summary>
        public Action[] InputActions { get; set; }

        /// <summary>
        /// Indicates the input handling rules for the current state. Optional.
        /// </summary>
        public Input Input { get; set; }

        /// <summary>
        /// Determine the actions that should be executed after receiving the user input. Optional.
        /// </summary>
        public Action[] OutputActions { get; set; }

        /// <summary>
        /// Determines the possible outputs and its conditions from the current state. Optional.
        /// Array of <see cref="Output"/>. Optional.
        /// </summary>
        public Output[] Outputs { get; set; }
    }
}
