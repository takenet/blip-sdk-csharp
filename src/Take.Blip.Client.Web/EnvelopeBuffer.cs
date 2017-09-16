using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Lime.Protocol;

namespace Take.Blip.Client.Web
{
    public sealed class EnvelopeBuffer : IEnvelopeBuffer
    {
        private readonly BufferBlock<Envelope> _buffer;

        public EnvelopeBuffer()
        {
            _buffer = new BufferBlock<Envelope>(new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded
            });
        }

        public Task SendAsync(Envelope envelope, CancellationToken cancellationToken)
            => _buffer.SendAsync(envelope, cancellationToken);

        public Task<Envelope> ReceiveAsync(CancellationToken cancellationToken)
            => _buffer.ReceiveAsync(cancellationToken);
    }
}
