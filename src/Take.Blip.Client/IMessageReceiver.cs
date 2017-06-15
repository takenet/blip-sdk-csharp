using Lime.Protocol;

namespace Take.Blip.Client
{
    /// <summary>
    /// Defines a service for receiving and handling <see cref="Message"/> envelopes from the connection.
    /// </summary>
    public interface IMessageReceiver : IEnvelopeReceiver<Message>
    {
    }
}
