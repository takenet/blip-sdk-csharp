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
        JsonDocument jsonDocuments;

        public OptionNativeContentReceiver(ISender sender)
        {
            _sender = sender;         
        }
        
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            JsonDocument jsonDocuments = new JsonDocument();
            jsonDocuments.Add("text", "hello, world!");//exemplo funcional no messenger

            await _sender.SendMessageAsync(jsonDocuments, message.From, cancellationToken);
        }
    }

}
