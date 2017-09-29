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
            var imageUri =
                new Uri("http://2.bp.blogspot.com/-pATX0YgNSFs/VP-82AQKcuI/AAAAAAAALSU/Vet9e7Qsjjw/s1600/Cat-hd-wallpapers.jpg");
            var previewUri =
                new Uri("https://encrypted-tbn3.gstatic.com/images?q=tbn:ANd9GcS8qkelB28RstsNxLi7gbrwCLsBVmobPjb5IrwKJSuqSnGX4IzX");

            var imageMediaLink = new MediaLink
            {
                Title = "Gato",
                Text = "Segue uma imagem de um gato",
                Size = 227791,
                Type = MediaType.Parse("image/jpeg"),
                PreviewUri = imageUri,
                Uri = imageUri,
                AspectRatio = "1:1"
            };

            await _sender.SendMessageAsync(imageMediaLink, message.From, cancellationToken);

            var audioMediaLink = new MediaLink
            {
                Title = "Audio",
                Type = MediaType.Parse("audio/mp3"),
                Uri = new Uri("http://blaamandagjazzband.dk/jazz/mp3/basin_street_blues.mp3"),
                Size = 3124123,
                AspectRatio = "1:1"
            };

            await _sender.SendMessageAsync(audioMediaLink, message.From, cancellationToken);
        }
    }
}
