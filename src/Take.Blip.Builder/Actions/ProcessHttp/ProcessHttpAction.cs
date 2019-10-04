using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Actions.ProcessHttp
{
    public sealed class ProcessHttpAction : ActionBase<ProcessHttpSettings>, IDisposable
    {
        public static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(60);

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
                    AddBotIdentityToHeaders(httpRequestMessage, context);

                    using (var cts = new CancellationTokenSource(settings.RequestTimeout ?? DefaultRequestTimeout))
                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                    using (var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, linkedCts.Token).ConfigureAwait(false))
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
            if (ShouldAddHeader("processHttpAddUserToRequestHeader", context))
            {
                httpRequestMessage.Headers.Add("X-Blip-User", context.UserIdentity);
            }
        }

        /// <summary>
        /// Add 'X-Blip-Bot' header to request with the Bot's name as its value, if there is 
        /// a configuration variable 'processHttpAddBotIdentityToRequestHeader' set to true
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <param name="context"></param>
        private void AddBotIdentityToHeaders(HttpRequestMessage httpRequestMessage, IContext context)
        {
            if (ShouldAddHeader("processHttpAddBotIdentityToRequestHeader", context))
            {
                httpRequestMessage.Headers.Add("X-Blip-Bot", context.OwnerIdentity);
            }
        }

        /// <summary>
        /// Fully checks if a given config header should be added or not
        /// </summary>
        /// <param name="header">Config header to check for</param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static bool ShouldAddHeader(string header, IContext context)
        {
            return context.Flow.Configuration != null &&
                context.Flow.Configuration.TryGetValue(header, out string identifierHeaderValue) &&
                bool.TryParse(identifierHeaderValue, out bool sendBotIdentifier) &&
                sendBotIdentifier;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}