using System;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Tunnel
{
    public static class EnvelopeExtensions
    {
        public static bool TryGetTunnelInformation<T>(this T envelope, out Takenet.Iris.Messaging.Resources.Tunnel tunnelInformation) where T : Envelope
        {
            if (envelope.From?.Domain == null ||
                !envelope.From.Domain.Equals(TunnelExtension.TunnelAddress.Domain, StringComparison.OrdinalIgnoreCase))
            {
                tunnelInformation = null;
                return false;
            }

            try
            {
                tunnelInformation = new Takenet.Iris.Messaging.Resources.Tunnel()
                {
                    Owner = envelope.Metadata.GetValueOrDefault(TunnelExtension.TUNNEL_OWNER_METADATA_KEY),
                    Originator = envelope.Metadata.GetValueOrDefault(TunnelExtension.TUNNEL_ORIGINATOR_METADATA_KEY),
                    Destination = envelope.From.ToIdentity()
                };
                return true;
            }
            catch (ArgumentException)
            {
                tunnelInformation = null;
                return false;
            }
        }
    }
}