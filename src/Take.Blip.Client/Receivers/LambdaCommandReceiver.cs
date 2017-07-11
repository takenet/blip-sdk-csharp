using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Receivers
{
    public class LambdaCommandReceiver : ICommandReceiver
    {
        private Func<Command, CancellationToken, Task> OnCommandReceived { get; }

        public LambdaCommandReceiver(Func<Command, CancellationToken, Task> onCommandReceived)
        {
            if (onCommandReceived == null) throw new ArgumentNullException(nameof(onCommandReceived));
            OnCommandReceived = onCommandReceived;
        }
        
        public Task ReceiveAsync(Command envelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            return OnCommandReceived?.Invoke(envelope, cancellationToken);
        }
    }
}
