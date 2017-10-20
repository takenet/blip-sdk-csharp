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
    public class OptionRedirectMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionRedirectMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Document document;
            if (message.Content.ToString().Equals("red1"))
                document = getRedirect();
            else
                document = getRedirectWithContext();

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

        public Redirect getRedirect()
        {
            var document = new Redirect
            {
                Address = "atendimento"
            };
            return document;
        }

        public Redirect getRedirectWithContext()
        {
            var document = new Redirect
                {
                    Address = "mysdkbot@msging.net",
                    Context = new DocumentContainer {
                        Value = new PlainText {
                            Text = "Get Started"
                        }
                    }
                };
            return document;
        }
    }
}