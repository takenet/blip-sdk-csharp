using Lime.Protocol;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Receivers
{
    public class AggregateEnvelopeReceiver<TEnvelope> : IEnvelopeReceiver<TEnvelope> 
        where TEnvelope : Envelope
    {
        private readonly IEnvelopeReceiver<TEnvelope>[] _receivers;

        public AggregateEnvelopeReceiver(params IEnvelopeReceiver<TEnvelope>[] receivers)
        {
            if (receivers == null) throw new ArgumentNullException(nameof(receivers));
            _receivers = receivers;
        }

        public virtual Task ReceiveAsync(TEnvelope envelope, CancellationToken cancellationToken = default(CancellationToken))
            => Task.WhenAll(_receivers.Select(r => r.ReceiveAsync(envelope, cancellationToken)));
    }
}
