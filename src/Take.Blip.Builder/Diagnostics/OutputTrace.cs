using System.Runtime.Serialization;

namespace Take.Blip.Builder.Diagnostics
{
    /// <summary>
    /// Represents the output of a state transition in the monitoring trace, including the target state and the number of conditions evaluated.
    /// </summary>
    public class OutputTrace : Trace
    {
        /// <summary>
        /// Gets or sets the identifier of the target state for this output.
        /// </summary>
        [DataMember(Name = "stateId")]
        public string StateId { get; set; }

        /// <summary>
        /// Gets or sets the number of conditions evaluated for this output transition.
        /// </summary>
        [DataMember(Name = "conditionsCount")]
        public int ConditionsCount { get; set; }
    }
}
