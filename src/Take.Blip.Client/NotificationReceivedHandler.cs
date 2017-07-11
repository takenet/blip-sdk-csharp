using Lime.Protocol;

namespace Take.Blip.Client
{
    internal sealed class NotificationReceivedHandler : EnvelopeReceivedHandler<Notification>
    {
        public NotificationReceivedHandler(EnvelopeReceiverManager registrar)
            : base(registrar)
        {
        }
    }
}