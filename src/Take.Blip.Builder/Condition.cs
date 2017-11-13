using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Output condition abstraction
    /// </summary>
    public class Condition
    {
        /// <summary>
        /// The variable name of the conversation context to be evaluated. Required.
        /// </summary>
        public string Variable { get; set; }

        /// <summary>
        /// The type of the variable comparison. Optional.
        /// </summary>
        public ConditionComparison Comparison { get; set; }

        /// <summary>
        /// The value to be used by the comparison with the context value. Required.
        /// </summary>
        public string Value { get; set; }
    }
}
