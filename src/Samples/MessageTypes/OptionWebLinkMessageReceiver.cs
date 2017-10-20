using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionWebLinkMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionWebLinkMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var url = new Uri("http://limeprotocol.org/content-types.html#web-link");
            var previewUri =
                new Uri(
                    "https://techbeacon.scdn7.secure.raxcdn.com/sites/default/files/styles/article_hero_image/public/documents-stack-documentation-agile-devops.jpg?itok=cFDq9Y95");

            var document = new WebLink
            {
                Text = "Here is a documentation weblink",
                Target = WebLinkTarget.Self,
                PreviewUri = previewUri,
                Uri = url
            };

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }
    }
}
