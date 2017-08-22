using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.Web
{
    public sealed class WebTransport : TransportBase, ITransport
    {
        private const string DefaultBaseUrl = "https://msging.net";

        private readonly IEnvelopeBuffer _envelopeBuffer;
        private readonly IEnvelopeSerializer _serializer;
        private readonly Application _applicationSettings;

        public WebTransport(IEnvelopeBuffer envelopeBuffer, IEnvelopeSerializer serializer, Application applicationSettings)
        {
            _envelopeBuffer = envelopeBuffer;
            _serializer = serializer;
            _applicationSettings = applicationSettings;
        }

        public override Task SendAsync(Envelope envelope, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<Envelope> ReceiveAsync(CancellationToken cancellationToken) 
            => _envelopeBuffer.ReceiveAsync(cancellationToken);

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
