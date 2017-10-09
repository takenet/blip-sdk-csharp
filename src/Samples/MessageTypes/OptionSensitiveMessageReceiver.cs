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
            Document document;
            if (message.Content.ToString().Equals("se1"))
                document = getSensitivePassword();
            else
                document = getSensitiveWeblink();

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

        public Document getSensitivePassword()
        {
            var document = new SensitiveContainer
            {
                Value = "Your password is 123456"
            };
            return document;
        }

        public Document getSensitiveWeblink()
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
            return document;
        }
    }
}
