using System;
using System.Collections.Generic;
using System.Text;
using Lime.Messaging.Contents;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Abstraction of user input
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Indicates that the state should not expect an user input to evalute its outputs.
        /// </summary>
        public bool Bypass { get; set; }

        /// <summary>
        /// Indicates where the user input value will be saved. Optional
        /// </summary>
        public string Variable { get; set; }

        /// <summary>
        /// Input validation rule. Optional
        /// </summary>
        public InputValidation Validation { get; set; }
    }
}
