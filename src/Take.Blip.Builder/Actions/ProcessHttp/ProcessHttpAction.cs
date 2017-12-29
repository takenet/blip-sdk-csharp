using System;
using System.Net.Http;
using System.Text;
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

        public string Type => nameof(ProcessHttp);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var processHttpSettings = settings.ToObject<ProcessHttpSettings>();

            if (processHttpSettings.Uri == null) throw new ArgumentException($"The '{nameof(ProcessHttpSettings.Uri)}' settings value is required for '{nameof(ProcessHttpAction)}' action");
            if (processHttpSettings.Method == null) throw new ArgumentException($"The '{nameof(ProcessHttpSettings.Method)}' settings value is required for '{nameof(ProcessHttpAction)}' action");

            var httpRequestMessage = new HttpRequestMessage(
                new HttpMethod(processHttpSettings.Method), processHttpSettings.Uri);

            if (processHttpSettings.Headers != null)
            {
                foreach (var header in processHttpSettings.Headers)
                {
                    httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(processHttpSettings.Body))
            {
                string contentType = null;
                processHttpSettings.Headers?.TryGetValue("Content-Type", out contentType);
                httpRequestMessage.Content = new StringContent(processHttpSettings.Body, Encoding.UTF8, contentType ?? "application/json");
            }

            var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

            // Set the responses variables
            if (!string.IsNullOrWhiteSpace(processHttpSettings.StatusVariable))
            {
                await context.SetVariableAsync(processHttpSettings.StatusVariable,
                    ((int) httpResponseMessage.StatusCode).ToString(), cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(processHttpSettings.BodyVariable))
            {                
                var body = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    await context.SetVariableAsync(processHttpSettings.BodyVariable, body, cancellationToken);
                }
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
