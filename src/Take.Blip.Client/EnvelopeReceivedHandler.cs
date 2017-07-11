using Lime.Protocol;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Take.Blip.Client
{
    internal abstract class EnvelopeReceivedHandler<TEnvelope> where TEnvelope : Envelope, new()
    {
        private readonly EnvelopeReceiverManager _registrar;
        private readonly ActionBlock<Tuple<TEnvelope, CancellationToken>> _envelopeActionBlock;

        protected EnvelopeReceivedHandler(EnvelopeReceiverManager registrar)
        {
            _registrar = registrar;
            _envelopeActionBlock = new ActionBlock<Tuple<TEnvelope, CancellationToken>>(async item =>
            {
                try
                {
                    await CallReceiversAsync(item.Item1, item.Item2);
                }
                catch (Exception ex)
                {
                    //TODO: Create a ILogger interface to notify about errors on EnvelopeProcessor.
                    Trace.TraceError(ex.ToString());
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded,
                BoundedCapacity = DataflowBlockOptions.Unbounded
            });
        }

        public Task<bool> HandleAsync(TEnvelope envelope, CancellationToken cancellationToken) =>
            _envelopeActionBlock.SendAsync(new Tuple<TEnvelope, CancellationToken>(envelope, cancellationToken), cancellationToken);

        protected virtual async Task CallReceiversAsync(TEnvelope envelope, CancellationToken cancellationToken)
        {
            var receivers = await _registrar.GetReceiversAsync(envelope);
            // Gets the first non empty group, ordered by priority
            var receiverGroup = receivers
                                .GroupBy(r => r.Priority)
                                .OrderBy(r => r.Key)
                                .First(r => r.Any());

            await Task.WhenAll(
                    receiverGroup.Select(r => CallReceiverAsync(r.ReceiverFactory(), envelope, cancellationToken)));
        }

        protected Task CallReceiverAsync(IEnvelopeReceiver<TEnvelope> envelopeReceiver, TEnvelope envelope, CancellationToken cancellationToken)
        {
            return envelopeReceiver.ReceiveAsync(envelope, cancellationToken);
        }
    }
}
