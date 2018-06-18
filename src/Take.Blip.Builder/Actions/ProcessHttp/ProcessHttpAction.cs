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
    public sealed class ProcessHttpAction : ActionBase<ProcessHttpSettings>, IDisposable
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;

        public ProcessHttpAction(IHttpClient httpClient, ILogger logger)
            : base(nameof(ProcessHttp))
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public override async Task ExecuteAsync(IContext context, ProcessHttpSettings settings, CancellationToken cancellationToken)
        {
            var responseStatus = 0;
            string responseBody = null;
            try
            {
                using (var httpRequestMessage =
                    new HttpRequestMessage(new HttpMethod(settings.Method), settings.Uri))
                {
                    if (settings.Headers != null)
                    {
                        foreach (var header in settings.Headers)
                        {
                            httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(settings.Body))
                    {
                        string contentType = null;
                        settings.Headers?.TryGetValue("Content-Type", out contentType);
                        httpRequestMessage.Content = new StringContent(settings.Body, Encoding.UTF8,
                            contentType ?? "application/json");
                    }

                    await AddUserToHeaderAsync(httpRequestMessage, context, cancellationToken);

                    using (var httpResponseMessage =
                        await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false))
                    {
                        responseStatus = (int)httpResponseMessage.StatusCode;
                        if (!string.IsNullOrWhiteSpace(settings.ResponseBodyVariable))
                        {
                            responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"An exception occurred while processing HTTP action: {ex.Message}");
            }

            // Set the responses variables
            if (!string.IsNullOrWhiteSpace(settings.ResponseStatusVariable))
            {
                await context.SetVariableAsync(settings.ResponseStatusVariable,
                    responseStatus.ToString(), cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(settings.ResponseBodyVariable) &&
                !string.IsNullOrWhiteSpace(responseBody))
            {
                await context.SetVariableAsync(settings.ResponseBodyVariable, responseBody, cancellationToken);
            }
        }

        private async Task AddUserToHeaderAsync(HttpRequestMessage httpRequestMessage, IContext context, CancellationToken cancellationToken)
        {
            var userHeaderValue = await context.GetVariableAsync("config.processHttp.addUserToRequestHeader", cancellationToken);
            if (bool.TryParse(userHeaderValue, out bool sendUserHeader) && sendUserHeader)
            {
                httpRequestMessage.Headers.Add("X-Blip-User", context.User);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
