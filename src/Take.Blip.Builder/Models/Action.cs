﻿using System.ComponentModel.DataAnnotations;
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
        /// The conditions of the conversation context to be evaluated in order to consider the current action valid. Optional.
        /// </summary>
        public Condition[] Conditions { get; set; }

        /// <summary>
        /// Gets the timeout for executing the action, in seconds. Optional.
        /// </summary>
        public double? Timeout { get; set; }

        /// <summary>
        /// Indicates that the input processing should continue if any error occur in the action execution.
        /// </summary>
        public bool ContinueOnError { get; set; }

        /// <summary>
        /// TODO: Indicates if the action should be executed asynchronously, without blocking the input processing.
        /// </summary>
        public bool ExecuteAsynchronously { get; set; }
        
        /// <summary>
        /// The action type name. Required.
        /// </summary>
        [Required(ErrorMessage = "The action type is required")]
        public string Type { get; set; }

        /// <summary>
        /// The action settings for the specified type. Optional.
        /// </summary>
        public JRaw Settings { get; set; }

        public void Validate()
        {
            this.ValidateObject();           
        }

        public ActionTrace ToTrace()
        {
            return new ActionTrace
            {
                Order = Order,
                Type = Type,
                ContinueOnError = ContinueOnError
            };
        }
    }
}
