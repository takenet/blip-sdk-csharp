using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;

namespace Take.Blip.Client.Receivers
{

    public abstract class UnsupportedEnvelopeReceiver<TEnvelope> : IEnvelopeReceiver<TEnvelope>
        where TEnvelope : Envelope
    {
        private Reason _reason;

        protected UnsupportedEnvelopeReceiver(Reason reason)
        {
            _reason = reason;
        }

        public virtual Task ReceiveAsync(TEnvelope enevelope, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(enevelope.Id) || enevelope.Id == Guid.Empty.ToString())
            {
                throw new LimeException(
                    _reason);
            }

            return Task.CompletedTask;
        }
    }
}