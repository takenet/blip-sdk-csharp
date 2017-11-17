using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Defines an action to be executed when a state is activated.
    /// </summary>
    public class Action
    {
        /// <summary>
        /// The action execution order, relative to the others in the same state. Optional.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The action type name.
        /// </summary>
        [Required]
        public string Type { get; set; }

        /// <summary>
        /// The action settings for the specified type.
        /// </summary>
        public IDictionary<string, object> Settings { get; set; }
    }
}
