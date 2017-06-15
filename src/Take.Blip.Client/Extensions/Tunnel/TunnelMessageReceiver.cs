using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Tunnel
{
    public class TunnelMessageReceiver : TunnelEnvelopeReceiver<Message>, IMessageReceiver
    {
        public TunnelMessageReceiver(ISender sender)
            : base(sender.SendMessageAsync)
        {
        }
    }
}
