using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions.ProcessHttp
{
    public sealed class ProcessHttpAction: IAction, IDisposable
    {
        private readonly IHttpClient _httpClient;

        public ProcessHttpAction(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string Type => "ProcessHttp";

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var processHttpSettings = settings.ToObject<ProcessHttpSettings>();

            var httpRequestMessage = new HttpRequestMessage(
                new HttpMethod(processHttpSettings.Method), processHttpSettings.Url);

            if (processHttpSettings.Headers != null)
            {
                foreach (var header in processHttpSettings.Headers)
                {
                    httpRequestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(processHttpSettings.Body))
            {
                httpRequestMessage.Content = new StringContent(processHttpSettings.Body);
            }

            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

            // Set the responses variables
            if (string.IsNullOrWhiteSpace(processHttpSettings.Variable)) return;
            await context.SetVariableAsync($"{processHttpSettings.Variable}.status", httpResponseMessage.StatusCode.ToString(), cancellationToken);

            var body = await httpResponseMessage.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(body))
            {
                await context.SetVariableAsync($"{processHttpSettings.Variable}.body", body, cancellationToken);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
