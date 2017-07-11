using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Client.Activation
{
    public class ApplicationReceiver : SettingsContainer
    {
        /// <summary>
        /// Gets or sets the receiver priority related to the others. 
        /// Lower values have higher priority. 
        /// This value can be repeated among receivers. 
        /// In this cases, the receivers are evaluated in parallel.
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the required caller state to ensure the validity of the receiver.
        /// </summary>
        /// <value>
        /// The state filter.
        /// </value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the state of the caller to be set after the execution of the receiver.
        /// </summary>
        /// <value>
        /// The state to be set.
        /// </value>
        public string OutState { get; set; }

        /// <summary>
        /// Gets or sets the receiver .NET type. 
        /// The type constructor must be parameterless or receive only a <see cref="IServiceProvider"/> instance plus a <see cref="IDictionary{TKey,TValue}"/> settings instance.
        /// </summary>
        /// <value>
        /// The type of the receiver.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the sender filter. It can be a regex.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the destination filter. It can be a regex.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        public string Destination { get; set; }

        /// <summary>
        /// Gets or sets the culture filter.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the message to be sent in case of the occurrences of the specified envelope filter.
        /// This overrides the receiver type definition, if present.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public DocumentDefinition Response { get; set; }

        /// <summary>
        /// Gets or sets the address for forwarding the envelope in a tunnel.
        /// </summary>
        public string ForwardTo { get; set; }
    }
}
