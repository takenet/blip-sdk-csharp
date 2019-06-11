using System;
using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Network;

namespace Take.Blip.Client.Extensions.Tunnel
{
    /// <summary>
    /// Allows forwarding envelopes to an identity.
    /// </summary>
    public interface ITunnelExtension
    {
        Task<Takenet.Iris.Messaging.Resources.Tunnel> GetTunnelAsync(Identity tunnelIdentity, CancellationToken cancellationToken);
    
        Task<Node> ForwardMessageAsync(Message message, Identity destination, CancellationToken cancellationToken);

        Task<Node> ForwardNotificationAsync(Notification notification, Identity destination, CancellationToken cancellationToken);
    }

    public static class TunnelExtensionExtensions
    {
        public static async Task<Takenet.Iris.Messaging.Resources.Tunnel> TryGetTunnelForMessageAsync(
            this ITunnelExtension tunnelExtension,
            Message message,
            CancellationToken cancellationToken)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (message.From?.Domain == null ||
                !message.From.Domain.Equals(TunnelExtension.TunnelAddress.Domain, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (message.TryGetTunnelInformation(out var tunnel))
            {
                return tunnel;
            }

            try
            {
                return await tunnelExtension.GetTunnelAsync(message.From.ToIdentity(), cancellationToken);
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return null;
            }
        }
    }
}
