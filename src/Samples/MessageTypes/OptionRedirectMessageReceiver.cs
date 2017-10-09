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

            var document = new Redirect
            {
                Address = new Node("1545282125497371", "@messenger.gw.msging.net", null)
            };

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }

    public class SpecificRedirectPassingContext : IMessageReceiver
        {
            private readonly ISender _sender;

            public SpecificRedirectPassingContext(ISender sender)
            {
                _sender = sender;
            }

            public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
            {
                var document = new Redirect
                {
                    Address = new Node("1545282125497371", "@messenger.gw.msging.net", null),
                    Context = new DocumentContainer {
                        Value = new PlainText {
                            Text = "Get Started"
                        }
                    }
                };

                await _sender.SendMessageAsync(document, message.From, cancellationToken);
            }
        }
}
