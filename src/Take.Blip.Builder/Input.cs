using System;
using System.Collections.Generic;
using System.Text;
using Lime.Messaging.Contents;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines rules for handling the user input in a <see cref="State"/>.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Indicates that the state should not expect an user input to evalute its outputs.
        /// </summary>
        public bool Bypass { get; set; }

        /// <summary>
        /// Indicates the variable name where the user input value should be saved. Optional.
        /// </summary>
        public string Variable { get; set; }

        /// <summary>
        /// Determine the rules for validate the user input. Optional.
        /// </summary>
        public InputValidation Validation { get; set; }
    }
}
