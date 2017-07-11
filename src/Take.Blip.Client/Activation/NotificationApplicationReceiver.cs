using System;
using System.Collections.Generic;
using System.Text;
using Lime.Protocol;

namespace Take.Blip.Client.Activation
{
    public class NotificationApplicationReceiver : ApplicationReceiver
    {
        /// <summary>
        /// Gets or sets the type of the event. 
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public Event? EventType { get; set; }
    }
}
