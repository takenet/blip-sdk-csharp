using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Diagnostics;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Defines an action to be executed when a state is activated.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Type) + "}")]
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

        public ActionTrace ToTrace()
        {
            return new ActionTrace
            {
                Order = Order,
                Type = Type
            };
        }
    }
}
