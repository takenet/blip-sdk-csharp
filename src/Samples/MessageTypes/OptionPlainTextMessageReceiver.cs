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
            var plainText = new PlainText
            {
                Text = "... Inspiração, e um pouco de café! E isso me basta!"
            };
            await _sender.SendMessageAsync(plainText, message.From, cancellationToken);
        }

    }
}
