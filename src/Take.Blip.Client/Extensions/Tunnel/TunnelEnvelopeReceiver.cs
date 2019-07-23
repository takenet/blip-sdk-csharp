using Lime.Protocol;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Take.Blip.Client.Extensions.Tunnel
{
    /// <summary>
    /// Receives envelopes from the tunnel address (in the [destination]@tunnel.msging.net/[originator] format)
    /// and forwards to the original sender.
    /// </summary>
    /// <typeparam name="TEnvelope"></typeparam>
    public class TunnelEnvelopeReceiver<TEnvelope> : IEnvelopeReceiver<TEnvelope> where TEnvelope : Envelope, new()
    {
        private readonly Func<TEnvelope, CancellationToken, Task> _senderFunc;

        public TunnelEnvelopeReceiver(Func<TEnvelope, CancellationToken, Task> senderFunc)
        {
            _senderFunc = senderFunc;
        }

        public virtual async Task ReceiveAsync(TEnvelope envelope, CancellationToken cancellationToken = default)
        {
            if (!(envelope.From?.Domain?.Equals(TunnelExtension.TunnelAddress.Domain, StringComparison.OrdinalIgnoreCase) ?? false)
                || envelope.From?.Instance == null)
            {
                throw new ArgumentException("Invalid envelope destination for the tunnel receiver. Please check the configured filter.");
            }

            // Retrieve the original destination
            var originator = Identity.Parse(Uri.UnescapeDataString(envelope.From.Instance));
            var originatorEnvelope = envelope.ShallowCopy();
            originatorEnvelope.From = null;
            originatorEnvelope.To = originator.ToNode();
            await _senderFunc(originatorEnvelope, cancellationToken);
        }
    }
}
