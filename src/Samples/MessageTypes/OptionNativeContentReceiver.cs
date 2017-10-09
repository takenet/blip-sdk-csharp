using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionNativeContentReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private  readonly JsonDocument JsonDocuments;

        public OptionNativeContentReceiver(ISender sender)
        {
            _sender = sender;         
        }
        
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            JsonDocument JsonDocuments = new JsonDocument();
            JsonDocuments.Add("text", "hello, world!");

            await _sender.SendMessageAsync(JsonDocuments, message.From, cancellationToken);
        }
    }

}
