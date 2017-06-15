using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;

namespace Takenet.MessagingHub.Client.Sender
{
    [Obsolete("Use the ISender interface instead")]
    public interface IMessagingHubSender : IEnvelopeSender
    {
    }

    public static class MessagingHubSenderExtensions
    {
        [Obsolete("Use the ProcessCommandAsync method instead")]
        public static Task<Command> SendCommandAsync(this IMessagingHubSender sender, Command command, CancellationToken cancellationToken = default(CancellationToken))
            => sender.ProcessCommandAsync(command, cancellationToken);
    }
}
