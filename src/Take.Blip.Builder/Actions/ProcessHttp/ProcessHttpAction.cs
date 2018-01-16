using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Actions.ProcessHttp
{
    public sealed class ProcessHttpAction: IAction, IDisposable
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        public ProcessHttpAction(IHttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public string Type => nameof(ProcessHttp);

        public async Task ExecuteAsync(IContext context, JObject settings, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var processHttpSettings = settings.ToObject<ProcessHttpSettings>();
            if (processHttpSettings.Uri == null)
            {
                throw new ArgumentException(
                    $"The '{nameof(ProcessHttpSettings.Uri)}' settings value is required for '{nameof(ProcessHttpAction)}' action");
            }
            if (processHttpSettings.Method == null)
            {
                throw new ArgumentException(
                    $"The '{nameof(ProcessHttpSettings.Method)}' settings value is required for '{nameof(ProcessHttpAction)}' action");
            }

            var responseStatus = 0;
            string responseBody = null;
            try
            {
                var httpRequestMessage =
                    new HttpRequestMessage(new HttpMethod(processHttpSettings.Method), processHttpSettings.Uri);
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
                    httpRequestMessage.Content = new StringContent(processHttpSettings.Body, Encoding.UTF8,
                        contentType ?? "application/json");
                }

                var httpResponseMessage =
                    await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);

                responseStatus = (int)httpResponseMessage.StatusCode;
                if (!string.IsNullOrWhiteSpace(processHttpSettings.ResponseBodyVariable))
                {
                    responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"An exception occurred while processing HTTP action: {ex.Message}");
            }

            // Set the responses variables
            if (!string.IsNullOrWhiteSpace(processHttpSettings.ResponseStatusVariable))
            {
                await context.SetVariableAsync(processHttpSettings.ResponseStatusVariable,
                    responseStatus.ToString(), cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(processHttpSettings.ResponseBodyVariable) &&
                !string.IsNullOrWhiteSpace(responseBody))
            {
                await context.SetVariableAsync(processHttpSettings.ResponseBodyVariable, responseBody, cancellationToken);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
