using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;
using Takenet.MessagingHub.Client.Sender;

namespace Navigation
{
    public class DateMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;
        private readonly CultureInfo _cultureInfo;
        private readonly string _messageTemplate;

        public DateMessageReceiver(ISender sender, IDictionary<string, object> settings)
        {
            _sender = sender;
            if (settings.ContainsKey("culture"))
            {            
                _cultureInfo = new CultureInfo((string)settings["culture"]);
            }
            else
            {
                _cultureInfo = CultureInfo.InvariantCulture;
            }

            _messageTemplate = (string)settings["message"];
        }

        public Task ReceiveAsync(Message envelope, CancellationToken cancellationToken = new CancellationToken())
        {
            return _sender.SendMessageAsync(string.Format(_messageTemplate, DateTime.Now.ToString("g", _cultureInfo)), envelope.From, cancellationToken);
        }
    }
}
