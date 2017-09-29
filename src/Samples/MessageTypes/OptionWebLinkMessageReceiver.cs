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
            var url = new Uri("https://pt.wikipedia.org/wiki/Caf%C3%A9");
            var previewUri =
                new Uri(
                    "https://upload.wikimedia.org/wikipedia/commons/thumb/c/c5/Roasted_coffee_beans.jpg/200px-Roasted_coffee_beans.jpg");

            var webLink = new WebLink
            {
                Text = "Café, a bebida sagrada!",
                PreviewUri = previewUri,
                Uri = url
            };

            await _sender.SendMessageAsync(webLink, message.From, cancellationToken);
        }
    }
}
