using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionSensitiveMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionSensitiveMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {

            var document = new SensitiveContainer
            {
                Value = "Your password is 123456"
            };

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }
    //Sending a Weblink sensitive
    public class SensitiveWeblinkMessage : IMessageReceiver
    {
        private readonly ISender _sender;

        public SensitiveWeblinkMessage(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var url = new Uri("https://mystore.com/checkout?ID=A8DJS1JFV98AJKS9");
            var document = new SensitiveContainer
            {
                Value = new WebLink
                {
                    Text = "Please follow this link for the checkout",
                    Uri = url
                }
            };

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }

}
