using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionPlainTextMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionPlainTextMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var document = new PlainText
            {
                Text = "Welcome to our service! How can I help you?"
            };
            await _sender.SendMessageAsync(document, message.From, cancellationToken);
        }

    }
}
