using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;
using Takenet.Iris.Messaging.Contents;

namespace MessageTypes
{
    public class OptionMultimidiaMenuMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionMultimidiaMenuMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var document = new DocumentSelect
            {
                Header = {
                    Value = new MediaLink{
                        Title = "Welcome to mad hatter",
                        Text = "Here we have the best hats for your head.",
                        Type = "iamge/jpeg",
                        Uri = new Uri("http://petersapparel.parseapp.com/img/item100-thumb.png"),
                        AspectRatio = "1.1"
                    }
                }
                
            };

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }

}
