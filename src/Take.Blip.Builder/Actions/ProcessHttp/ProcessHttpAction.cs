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

                    AddUserToHeaders(httpRequestMessage, context);

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

        /// <summary>
        /// Add 'X-Blip-User' header to request, with current user identity as its value, if there is 
        /// a configuration variable 'processHttpAddUserToRequestHeader' set to true
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <param name="context"></param>
        private void AddUserToHeaders(HttpRequestMessage httpRequestMessage, IContext context)
        {
            if (context.Flow.Configuration != null &&
                context.Flow.Configuration.TryGetValue("processHttpAddUserToRequestHeader", out string userHeaderValue) && 
                bool.TryParse(userHeaderValue, out bool sendUserHeader) && 
                sendUserHeader)
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
