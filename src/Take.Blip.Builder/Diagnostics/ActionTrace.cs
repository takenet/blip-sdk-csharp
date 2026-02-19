using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Take.Blip.Builder.Diagnostics
{

    /// <summary>
    /// Represents an action trace within a state, including order, type, parsed settings, and error handling flag.
    /// </summary>
    public class ActionTrace : Trace
    {
        /// <summary>
        /// Gets or sets the order of the action within the state.
        /// </summary>
        [DataMember(Name = "order")]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the type of the action.
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the parsed settings for the action.
        /// </summary>
        [DataMember(Name = "parsedSettings")]
        public JRaw ParsedSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to continue on error.
        /// </summary>
        [DataMember(Name = "continueOnError")]
        public bool ContinueOnError { get; set; }
    }
}
