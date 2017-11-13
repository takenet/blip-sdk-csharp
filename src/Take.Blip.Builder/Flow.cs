using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Builder
{
    /// <summary>
    /// The state machine.
    /// </summary>
    public class Flow
    {
        /// <summary>
        /// The flow states. Required.
        /// </summary>
        public State[] States { get; set; }
    }
}
