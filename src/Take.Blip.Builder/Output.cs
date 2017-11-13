using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines an output path from a state.
    /// </summary>
    public class Output
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
        public string StateId { get; set; }
    }
}
