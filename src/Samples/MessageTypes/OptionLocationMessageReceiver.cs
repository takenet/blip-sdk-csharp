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
            var location = new Location
            {
                Latitude = -22.121944,
                Longitude = -45.128889,
                Altitude = 1143
            };

            await _sender.SendMessageAsync(location, message.From, cancellationToken);
        }
    }
}
