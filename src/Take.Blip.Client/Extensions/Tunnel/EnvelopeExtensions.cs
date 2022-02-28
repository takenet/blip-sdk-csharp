using System;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Tunnel
{
    public static class EnvelopeExtensions
    {
        public static bool TryGetTunnelFromEnvelope<T>(this T envelope, out Takenet.Iris.Messaging.Resources.Tunnel tunnelInformation) where T : Envelope
        {
            Node fromNode = envelope.From;

            if (envelope.Metadata != null && envelope.Metadata.ContainsKey(Constants.ORIGINAL_SUBFLOW_REDIRECT_FROM))
            {
                envelope.Metadata.TryGetValue(Constants.ORIGINAL_SUBFLOW_REDIRECT_FROM, out string originalFrom);
                fromNode = Identity.Parse(originalFrom).ToNode();
            }

            if (fromNode?.Domain != null &&
                fromNode.Domain.Equals(TunnelExtension.TunnelAddress.Domain, StringComparison.OrdinalIgnoreCase) &&
                envelope.Metadata != null &&
                envelope.Metadata.TryGetValue(TunnelExtension.TUNNEL_OWNER_METADATA_KEY, out var owner) &&
                envelope.Metadata.TryGetValue(TunnelExtension.TUNNEL_ORIGINATOR_METADATA_KEY, out var originator))
            {
                tunnelInformation = new Takenet.Iris.Messaging.Resources.Tunnel()
                {
                    Owner = owner,
                    Originator = originator,
                    Destination = envelope.To?.ToIdentity()
                };
                return true;
            }

            tunnelInformation = null;
            return false;
        }
    }
}