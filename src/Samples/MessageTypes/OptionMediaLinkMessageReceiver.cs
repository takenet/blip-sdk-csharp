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
            else if (message.Content.ToString().Equals("ml2"))
                document = getAudio();
            else if (message.Content.ToString().Equals("ml3"))
                document = getGif();
            else if (message.Content.ToString().Equals("ml4"))
                document = getVideo();
            else
                document = getPDF();

            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

        public MediaLink getGif()
        {
            var imageUri =
                new Uri("http://i.giphy.com/14aUO0Mf7dWDXW.gif");

            var document = new MediaLink
            {
                Type = "image/gif",
                Uri = imageUri
            };
            return document;
        }

        public MediaLink getImage()
        {
            var imageUri = new Uri("http://2.bp.blogspot.com/-pATX0YgNSFs/VP-82AQKcuI/AAAAAAAALSU/Vet9e7Qsjjw/s1600/Cat-hd-wallpapers.jpg", UriKind.Absolute);
            var previewUri = new Uri("https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcS8qkelB28RstsNxLi7gbrwCLsBVmobPjb5IrwKJSuqSnGX4IzX", UriKind.Absolute);

            var document = new MediaLink
            {
                Title = "Cat",
                Text = "Here is a cat image for you!",
                Type = MediaType.Parse("image/jpeg"),
                AspectRatio = "1:1",
                Size = 227791,
                Uri = imageUri,
                PreviewUri = previewUri
            };

            return document;
        }

        public MediaLink getAudio()
        {
            var document = new MediaLink
            {
                Type = MediaType.Parse("audio/mp3"),
                Uri = new Uri("http://blaamandagjazzband.dk/jazz/mp3/basin_street_blues.mp3"),
            };
            return document;
        }

        public MediaLink getVideo()
        {
            var document = new MediaLink
            {
                Type = MediaType.Parse("video/mp4"),
                Uri = new Uri("http://techslides.com/demos/sample-videos/small.mp4"),
            };
            return document;
        }

        public MediaLink getPDF()
        {
            var document = new MediaLink
            {
                Uri = new Uri("http://www.adobe.com/content/dam/acom/en/devnet/acrobat/pdfs/pdf_open_parameters.pdf"),
            };
            return document;
        }
    }
}
