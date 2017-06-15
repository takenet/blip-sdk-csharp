using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Tunnel
{
    public class TunnelNotificationReceiver : TunnelEnvelopeReceiver<Notification>, INotificationReceiver
    {
        public TunnelNotificationReceiver(ISender sender)
            : base(sender.SendNotificationAsync)
        {
        }
    }
}
