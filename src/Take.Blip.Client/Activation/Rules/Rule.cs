using System.Collections.Generic;
using Lime.Protocol;

namespace Take.Blip.Client.Activation.Rules
{
    public class Rule
    {
        /// <summary>
        /// Gets or sets the receiver priority related to the others. 
        /// Lower values have higher priority. 
        /// This value can be repeated among receivers. 
        /// In this cases, the receivers are evaluated in parallel.
        /// </summary>
        public int Priority { get; set; }

        public When When { get; set; }

        public Do Do { get; set; }
    }


    public class When
    {
        /// <summary>
        /// Gets or sets the required caller state to ensure the validity of the receiver.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the culture filter.
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the message filter.
        /// </summary>
        public WhenMessage Message { get; set; }

        /// <summary>
        /// Gets or sets the notification filter.
        /// </summary>
        public WhenNotification Notification { get; set; }

        /// <summary>
        /// Gets or sets the command filter.
        /// </summary>
        public WhenCommand Command { get; set; }
    }

    public class WhenMessage : WhenEnvelope
    {
        /// <summary>
        /// Gets or sets the type filter. It can be a regex.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the content filter. It can be a regex.
        /// </summary>        
        public string Content { get; set; }
    }

    public class WhenNotification : WhenEnvelope
    {
        /// <summary>
        /// Gets or sets the type of the event. 
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        public Event? EventType { get; set; }
    }

    public class WhenCommand : WhenEnvelope
    {
        public CommandMethod? Method { get; set; }

        public string Uri { get; set; }

        public string Type { get; set; }
    }

    public class WhenEnvelope
    {
        /// <summary>
        /// Gets or sets the sender filter. It can be a regex.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the destination filter. It can be a regex.
        /// </summary>
        public string Destination { get; set; }
    }

    public class Do
    {
        public DoSendMessage SendMessage { get; set; }

        public DoInvokeReceiver InvokeReceiver { get; set; }

        public string DoSetState { get; set; }
    }

    public class DoSendMessage
    {
        /// <summary>
        /// Gets or sets the resource key to be used as content for the message.
        /// </summary>
        public string ResourceKey { get; set; }

        public string Id { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Type { get; set; }

        public object Content { get; set; }

        public IDictionary<string, string> Metadata { get; set; }
    }

    public class DoInvokeReceiver : SettingsContainer
    {
        public string Type { get; set; }
    }
}
