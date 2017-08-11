using Lime.Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Extensions.Tunnel
{
    public class TunnelExtension : ITunnelExtension
    {
        public static readonly Node TunnelAddress = Node.Parse($"postmaster@tunnel.{Constants.DEFAULT_DOMAIN}");

        private readonly ISender _sender;

        public TunnelExtension(ISender sender)
        {
            _sender = sender;
        }

        public Task<Node> ForwardMessageAsync(Message message, Identity destination, CancellationToken cancellationToken)
            => ForwardAsync(message, destination, _sender.SendMessageAsync, cancellationToken);

        public Task<Node> ForwardNotificationAsync(Notification notification, Identity destination, CancellationToken cancellationToken)
            => ForwardAsync(notification, destination, _sender.SendNotificationAsync, cancellationToken);

        private async Task<Node> ForwardAsync<TEnvelope>(TEnvelope envelope, Identity destination, Func<TEnvelope, CancellationToken, Task> senderFunc, CancellationToken cancellationToken)
            where TEnvelope : Envelope, new()
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if (envelope.From == null) throw new ArgumentException("The envelope 'from' value must be provided", nameof(envelope));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var tunnelAddress = new Node(
                Uri.EscapeDataString(destination.ToString()),
                TunnelAddress.Domain,
                Uri.EscapeDataString(envelope.From.ToString()));

            var tunnelEnvelope = envelope.ShallowCopy();
            tunnelEnvelope.From = null;
            tunnelEnvelope.To = tunnelAddress;

            await senderFunc(tunnelEnvelope, cancellationToken);
            return tunnelAddress;
        }
    }
}
