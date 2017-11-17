using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Defines an action to be executed when a state is activated.
    /// </summary>
    public class Action : IValidable
    {
        /// <summary>
        /// The action execution order, relative to the others in the same state. Optional.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// The action type name. Required.
        /// </summary>
        [Required(ErrorMessage = "The action type is required")]
        public string Type { get; set; }

        /// <summary>
        /// The action settings for the specified type. Optional.
        /// </summary>
        public JObject Settings { get; set; }

        public void Validate()
        {
            this.ValidateObject();
        }
    }
}
