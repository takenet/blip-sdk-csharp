using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionCollectionMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionCollectionMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var collection = new DocumentCollection
            {
                ItemType = DocumentContainer.MediaType,
                Items = new[]
                {
                    new DocumentContainer
                    {
                        Value = new PlainText { Text = "Option 1"}
                    },
                    // new DocumentContainer
                    // {
                    //     Value = Option2MessageReceiver.CreateMediaLink()
                    // },
                    // new DocumentContainer
                    // {
                    //     Value = Option3MessageReceiver.CreateWebLink()
                    // },
                    // new DocumentContainer
                    // {
                    //     Value = Option4MessageReceiver.CreateLocation()
                    // }
                }
            };
            await _sender.SendMessageAsync(collection, message.From, cancellationToken);
        }
    }
}
