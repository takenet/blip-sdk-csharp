using Lime.Protocol;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Diagnostics
{
    public class TraceProcessor : ITraceProcessor
    {
        private readonly IHttpClient _httpClient;

        public TraceProcessor(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task ProcessTraceAsync(TraceEvent traceEvent, CancellationToken cancellationToken)
        {
            if (traceEvent == null) throw new ArgumentNullException(nameof(traceEvent));

            switch (traceEvent.Settings.TargetType)
            {
                case TraceTargetType.Http:
                    return ProcessHttpTraceAsync(new Uri(traceEvent.Settings.Target), traceEvent.Trace, cancellationToken);
                    
                case TraceTargetType.Lime:
                    return ProcessLimeTraceAsync(traceEvent.Settings.Target, traceEvent.Trace, cancellationToken);

                default:
                    throw new NotSupportedException($"Unsupported trace target '{traceEvent.Settings.TargetType}'");
            }
        }

        private async Task ProcessHttpTraceAsync(Uri uri, InputTrace trace, CancellationToken cancellationToken)
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                var json = JsonConvert.SerializeObject(trace, JsonSerializerSettingsContainer.Settings);

                httpRequestMessage.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                using (var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false))
                {
                    httpResponseMessage.EnsureSuccessStatusCode();
                }
            }
        }

        private async Task ProcessLimeTraceAsync(Node node, InputTrace trace, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Lime trace target is not implemented");

        }
    }
}
