using System;
using System.Threading.Tasks;
using Lime.Messaging;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Transport.Tcp;
using Serilog;
using Serilog.Events;
using Take.Blip.Client.Extensions;

namespace Take.Blip.Client
{
    public class TcpTransportFactory : ITransportFactory
    {
        public static readonly IEnvelopeSerializer DefaultSerializer;

        static TcpTransportFactory()
        {
            DefaultSerializer = new EnvelopeSerializer(new DocumentTypeResolver().WithBlipDocuments());
        }

        private readonly IEnvelopeSerializer _envelopeSerializer;

        public TcpTransportFactory(IEnvelopeSerializer envelopeSerializer = null)
        {
            _envelopeSerializer = envelopeSerializer ?? DefaultSerializer;
        }

        public ITransport Create(Uri endpoint)
        {
            if (endpoint.Scheme != Uri.UriSchemeNetTcp)
            {
                throw new NotSupportedException($"Unsupported URI scheme '{endpoint.Scheme}'");
            }

            return new TcpTransport(traceWriter: new TraceWriter(), envelopeSerializer: _envelopeSerializer);
        }

        private class TraceWriter : ITraceWriter
        {
            public Task TraceAsync(string data, DataOperation operation)
            {
                Log.Logger?.Verbose("{Operation}: " + data, operation);
                return Task.CompletedTask;
            }

            public bool IsEnabled => Log.Logger != null && Log.Logger.IsEnabled(LogEventLevel.Verbose);
        }
    }
}