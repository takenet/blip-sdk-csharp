using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Sender;
using System.Diagnostics;
using Take.Blip.Client;

namespace Navigation
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        private readonly ISender _sender;

        public PlainTextMessageReceiver(ISender sender)
        {
            _sender = sender;
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Trace.TraceInformation($"From: {message.From} \tContent: {message.Content}");
            await _sender.SendMessageAsync("Pong!", message.From, cancellationToken);
        }
    }
}
