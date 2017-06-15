using Lime.Protocol;

namespace Take.Blip.Client
{

    /// <summary>
    /// Defines a service for receiving and handling <see cref="Notification"/> envelopes from the connection.
    /// </summary>
    public interface INotificationReceiver : IEnvelopeReceiver<Notification>
    {
    }
}
