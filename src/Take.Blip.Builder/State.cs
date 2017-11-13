using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace Take.Blip.Builder
{
    /// <summary>
    /// A block in the flow
    /// </summary>
    public class State
    {
        /// <summary>
        /// Unique id for the state. Required
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Indicates if this is the root state if the user has no active conversation. Optional.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Array of <see cref="Action"/>. Optional
        /// </summary>
        public Action[] Actions { get; set; }

        /// <summary>
        /// A <see cref="Builder.Input"/> setting. Optional.
        /// </summary>
        public Input Input { get; set; }

        /// <summary>
        /// Array of <see cref="Output"/>. Optional
        /// </summary>
        public Output[] Outputs { get; set; }
    }
}
