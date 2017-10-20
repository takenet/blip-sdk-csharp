using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionMediaLinkMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionMediaLinkMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Document document;

            if (message.Content.ToString().Equals("ml1"))
                document = getImage();
            else
                document = getAudio();

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

        public MediaLink getImage()
        {
            var imageUri =
                new Uri("http://2.bp.blogspot.com/-pATX0YgNSFs/VP-82AQKcuI/AAAAAAAALSU/Vet9e7Qsjjw/s1600/Cat-hd-wallpapers.jpg");
            var previewUri =
                new Uri("https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcS8qkelB28RstsNxLi7gbrwCLsBVmobPjb5IrwKJSuqSnGX4IzX");

            var imageMediaLink = new MediaLink
            {
                Title = "Cat",
                Text = "Here is a cat image for you!",
                Type = MediaType.Parse("image/jpeg"),
                AspectRatio = "1:1",
                Size = 227791,
                Uri = imageUri,
                PreviewUri = previewUri
            };
            return imageMediaLink;
        }

        public MediaLink getAudio()
        {
            var audioMediaLink = new MediaLink
            {
                Type = MediaType.Parse("audio/mp3"),
                Uri = new Uri("http://blaamandagjazzband.dk/jazz/mp3/basin_street_blues.mp3"),
                Size = 3124123,
            };
            return audioMediaLink;
        }
    }
}
