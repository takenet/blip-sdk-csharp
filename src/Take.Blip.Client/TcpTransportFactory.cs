using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Transport.Tcp;
using Serilog;
using Serilog.Events;

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
                Log.Logger?.Verbose("{Operation}: " + data, operation);
                return Task.CompletedTask;
            }

            public bool IsEnabled => Log.Logger != null && Log.Logger.IsEnabled(LogEventLevel.Verbose);
        }
    }
}
