using Lime.Protocol.Network;

namespace Take.Blip.Client
{
    /// <summary>
    /// Defines a service for sending message, command and notification envelopes throught an active connection.
    /// </summary>
    public interface ISender : IEstablishedSenderChannel, ICommandProcessor
    {
    }
}
