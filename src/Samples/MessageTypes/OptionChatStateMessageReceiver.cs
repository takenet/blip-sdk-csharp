using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Take.Blip.Client;

namespace MessageTypes
{
    public class OptionChatStateMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public OptionChatStateMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var chatState = new ChatState
            {
                State = ChatStateEvent.Composing
            };
            await _sender.SendMessageAsync(chatState, message.From, cancellationToken);
        }

    }
}
