using Lime.Protocol.Network;

namespace Take.Blip.Client
{
    /// <summary>
    /// Defines a service for sending messages, notifications and commands throught an active connection.
    /// </summary>
    public interface ISender : IEstablishedSenderChannel, ICommandProcessor
    {
    }
}
