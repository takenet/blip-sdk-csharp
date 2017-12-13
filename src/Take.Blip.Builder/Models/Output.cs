using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Defines an output path from a state.
    /// </summary>
    [DebuggerDisplay("{" + nameof(StateId) + "}")]
    public class Output : IValidable
    {
        /// <summary>
        /// The output execution order, relative to the others in the same state. Optional.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The conditions of the conversation context to be evaluated in order to consider the current output valid. Optional.
        /// </summary>
        public Condition[] Conditions { get; set; }

        /// <summary>
        /// The id of the state to be activated in case of success evaluation of the conditions. Required.
        /// </summary>
        [Required(ErrorMessage = "The output state id is required")]
        public string StateId { get; set; }

        public void Validate()
        {
            this.ValidateObject();

            if (Conditions != null)
            {
                foreach (var condition in Conditions)
                {
                    condition.Validate();
                }
            }
        }
    }
}
