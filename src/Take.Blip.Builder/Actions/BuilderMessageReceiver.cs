using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class BuilderMessageReceiver : IMessageReceiver
    {
        public Task ReceiveAsync(Message envelope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}