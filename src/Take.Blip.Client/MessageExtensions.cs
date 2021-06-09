using System.Collections.Generic;
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
            Event = @event,
            Metadata = new Dictionary<string, string>
            {
                { "#message.to", message.To },
                { "#message.uniqueId", message.GetMetadataKeyValue("$internalId")}
            }
        };

        private static string GetMetadataKeyValue(this Envelope envelope, string key)
        {
            if (envelope.Metadata == null)
            { 
                return null;
            }
            envelope.Metadata.TryGetValue(key, out var value);
            return value;
        }
    }
}