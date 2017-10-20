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

        public OptionNativeContentReceiver(ISender sender)
        {
            _sender = sender;         
        }
        
        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            JsonDocument document = new JsonDocument();
            document.Add("text", "hello, world!"); //exemplo funcional no messenger

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }

}
