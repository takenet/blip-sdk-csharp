using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;

namespace Take.Blip.Client.Web
{
    public sealed class WebTransport : TransportBase, ITransport
    {
        private readonly IEnvelopeBuffer _envelopeBuffer;

        public WebTransport(IEnvelopeBuffer envelopeBuffer)
        {
            _envelopeBuffer = envelopeBuffer;
        }

        public override Task SendAsync(Envelope envelope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<Envelope> ReceiveAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task PerformCloseAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task PerformOpenAsync(Uri uri, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override bool IsConnected { get; }
    }
}
