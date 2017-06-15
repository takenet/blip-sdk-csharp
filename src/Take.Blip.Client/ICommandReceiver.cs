using Lime.Protocol;

namespace Take.Blip.Client
{
    /// <summary>
    /// Defines a service for receiving and handling <see cref="Command"/> envelopes from the connection.
    /// </summary>
    public interface ICommandReceiver : IEnvelopeReceiver<Command>
    {
    }
}
