using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Receivers
{
    public class LambdaMessageReceiver : IMessageReceiver
    {
        private Func<Message, CancellationToken, Task> OnMessageReceived { get; }

        public LambdaMessageReceiver(Func<Message, CancellationToken, Task> onMessageReceived)
        {
            if (onMessageReceived == null) throw new ArgumentNullException(nameof(onMessageReceived));
            OnMessageReceived = onMessageReceived;
        }

        public Task ReceiveAsync(Message message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnMessageReceived?.Invoke(message, cancellationToken);
        }
    }
}