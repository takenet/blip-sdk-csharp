using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Transport.Tcp;
using Serilog;

namespace Take.Blip.Client
{
    public class TcpTransportFactory : ITransportFactory
    {
        public ITransport Create(Uri endpoint)
        {
            if (endpoint.Scheme != Uri.UriSchemeNetTcp)
            {
                throw new NotSupportedException($"Unsupported URI scheme '{endpoint.Scheme}'");
            }

            return new TcpTransport(traceWriter: new TraceWriter(), envelopeSerializer: new JsonNetSerializer());
        }

        private class TraceWriter : ITraceWriter
        {
            public Task TraceAsync(string data, DataOperation operation)
            {
                Trace.WriteLine(data, operation.ToString());
                return Task.CompletedTask;
            }

            public bool IsEnabled => Trace.Listeners.Count > 0;
        }
    }
}
