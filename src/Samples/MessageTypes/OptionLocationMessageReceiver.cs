using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionLocationMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionLocationMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Document document;

            if (message.Content.ToString().Equals("lo1"))
                document = getLocation();
            else
                document = getInputLocation();
            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

        public Document getLocation()
        {
            var location = new Location
            {
                Latitude = -19.919715,
                Longitude = -43.959753,
                Altitude = 853,
                Text = "Take's place"
            };

            return location;
        }

        public Document getInputLocation()
        {
            var location = new Input
            {
                Label = new DocumentContainer {
                    Value = new PlainText
                    {
                        Text = "Send your location please!"
                    }
                },
                Validation = new InputValidation {
                    Rule = InputValidationRule.Type,
                    Type = Location.MediaType
                }
            };

            return location;
        }

    }
}
