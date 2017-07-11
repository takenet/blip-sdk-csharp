using Lime.Protocol;

namespace Take.Blip.Client.Receivers
{

    /// <summary>
    /// Implements a <see cref="Notification"/> receiver that call multiple receivers.
    /// </summary>
    /// <seealso cref="Take.Blip.Client.Receivers.AggregateEnvelopeReceiver{Lime.Protocol.Notification}" />
    /// <seealso cref="Take.Blip.Client.INotificationReceiver" />
    public class AggregateNotificationReceiver : AggregateEnvelopeReceiver<Notification>, INotificationReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateNotificationReceiver"/> class.
        /// </summary>
        /// <param name="receivers">The receivers.</param>
        public AggregateNotificationReceiver(params INotificationReceiver[] receivers)
            : base(receivers)
        {

        }
    }
}
