using System;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Tunnel
{
    public static class EnvelopeExtensions
    {
        public static bool TryGetTunnelInformation<T>(this T envelope, out TunnelInformation tunnelInformation) where T : Envelope
        {
            if (envelope.From?.Domain == null ||
                !envelope.From.Domain.Equals(TunnelExtension.TunnelAddress.Domain, StringComparison.OrdinalIgnoreCase))
            {
                tunnelInformation = null;
                return false;
            }

            try
            {
                tunnelInformation = new TunnelInformation(
                    envelope.Metadata.GetValueOrDefault(TunnelExtension.TUNNEL_OWNER_METADATA_KEY),
                    envelope.Metadata.GetValueOrDefault(TunnelExtension.TUNNEL_ORIGINATOR_METADATA_KEY),
                    envelope.From.ToIdentity());
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