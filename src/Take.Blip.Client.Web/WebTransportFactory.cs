using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Serilog;
using Serilog.Events;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.Web
{
    public class WebTransportFactory : ITransportFactory
    {
        private readonly IEnvelopeBuffer _envelopeBuffer;
        private readonly IEnvelopeSerializer _envelopeSerializer;
        private readonly Application _application;

        public WebTransportFactory(IEnvelopeBuffer envelopeBuffer, IEnvelopeSerializer envelopeSerializer, Application application)
        {
            _envelopeBuffer = envelopeBuffer;
            _envelopeSerializer = envelopeSerializer;
            _application = application;
        }

        public ITransport Create(Uri endpoint)
        {
            if (endpoint.Scheme != Uri.UriSchemeHttp
                && endpoint.Scheme != Uri.UriSchemeHttps)
            {
                throw new NotSupportedException($"Unsupported URI scheme '{endpoint.Scheme}'");
            }

            return new WebTransport(_envelopeBuffer, _envelopeSerializer, _application, endpoint, traceWriter: new TraceWriter());
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
