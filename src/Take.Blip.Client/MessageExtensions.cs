using Lime.Protocol;

namespace Take.Blip.Client
{
    /// <summary>
    /// Extension methods for <see cref="Message"/>
    /// </summary>
    public static class MessageExtensions
    {
        public static Notification ToReceivedNotification(this Message message)
            => message.ToNotification(Event.Received);

        public static Notification ToConsumedNotification(this Message message)
            => message.ToNotification(Event.Consumed);

        public static Notification ToFailedNotification(this Message message, Reason reason)
        {
            var notification = message.ToNotification(Event.Failed);
            notification.Reason = reason;
            return notification;
        }

        public static Notification ToNotification(this Message message, Event @event) => new Notification
        {
            Id = message.Id,
            To = message.GetSender(),
            Event = @event
        };
    }
}