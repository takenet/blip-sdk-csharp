using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Receivers
{
    public class LambdaNotificationReceiver : INotificationReceiver
    {
        private Func<Notification, CancellationToken, Task> OnNotificationReceived { get; }

        public LambdaNotificationReceiver(Func<Notification, CancellationToken, Task> onNotificationReceived)
        {
            if (onNotificationReceived == null) throw new ArgumentNullException(nameof(onNotificationReceived));
            OnNotificationReceived = onNotificationReceived;
        }

        public Task ReceiveAsync(Notification notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnNotificationReceived?.Invoke(notification, cancellationToken);
        }
    }
}