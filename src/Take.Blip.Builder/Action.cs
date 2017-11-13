using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder
{
    /// <summary>
    /// An action to be executed when a state is activated.
    /// </summary>
    public class Action
    {
        /// <summary>
        /// The action execution order, relative to the others in the same state. Optional.
        /// </summary>
        public int Order { get; set; }
        
        /// <summary>
        /// The action type. Can be 'SendMessage', 'TrackEvent', 'ProcessHttp', 'AddToList', 'RemoveFromList'.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The action settings for the specified type.
        /// </summary>
        public JObject Settings { get; set; }
    }
}
