using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.Client.Activation
{
    public class MessageApplicationReceiver : ApplicationReceiver
    {
        /// <summary>
        /// Gets or sets the type of the media.
        /// </summary>
        /// <value>
        /// The type of the media.
        /// </value>
        public string MediaType { get; set; }

        /// <summary>
        /// Gets or sets the content filter. It can be a regex.
        /// </summary>
        /// <value>
        /// The text regex.
        /// </value>
        public string Content { get; set; }
    }
}
