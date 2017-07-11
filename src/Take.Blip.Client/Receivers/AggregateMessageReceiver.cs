using Lime.Protocol;

namespace Take.Blip.Client.Receivers
{

    /// <summary>
    /// Implements a <see cref="Message"/> receiver that call multiple receivers.
    /// </summary>
    /// <seealso cref="Take.Blip.Client.Receivers.AggregateEnvelopeReceiver{Lime.Protocol.Message}" />
    /// <seealso cref="Take.Blip.Client.IMessageReceiver" />
    public class AggregateMessageReceiver : AggregateEnvelopeReceiver<Message>, IMessageReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateMessageReceiver"/> class.
        /// </summary>
        /// <param name="receivers">The receivers.</param>
        public AggregateMessageReceiver(params IMessageReceiver[] receivers)
            : base(receivers)
        {

        }
    }
}
