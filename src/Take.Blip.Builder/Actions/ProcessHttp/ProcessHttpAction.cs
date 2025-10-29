using System;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Actions.ProcessHttp
{
    public sealed class ProcessHttpAction : ActionBase<ProcessHttpSettings>, IDisposable
    {
        private const string ADD_USER_KEY = "processHttpAddUserToRequestHeader";
        private const string ADD_BOT_KEY = "processHttpAddBotIdentityToRequestHeader";
        private const string SEND_HEADERS_TO_TRACE_COLLECTOR = "sendHeadersToTraceCollector";
        private const string ACTION_PROCESS_HTTP = "ProcessHttp";

        public static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(60);

        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IVariableReplacer _variableReplacer;
        private readonly ISensitiveInfoReplacer _sensitiveInfoReplacer;

        private static readonly string[] OUTPUT_PARAMETERS_NAME = new string[]
        {
            nameof(ProcessHttpSettings.ResponseStatusVariable).ToCamelCase(),
            nameof(ProcessHttpSettings.ResponseBodyVariable).ToCamelCase()
        };

        public ProcessHttpAction(IHttpClient httpClient, ILogger logger, IConfiguration configuration,  ISensitiveInfoReplacer sensitiveInfoReplacer, IVariableReplacer variableReplacer)
            : base(nameof(ProcessHttp), OUTPUT_PARAMETERS_NAME)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _sensitiveInfoReplacer = sensitiveInfoReplacer;
            _variableReplacer = variableReplacer;
        }

        public override async Task ExecuteAsync(IContext context, ProcessHttpSettings settings, CancellationToken cancellationToken)
        {
            var responseStatus = 0;
            string responseBody = null;
            try
            {
                bool isSuccessStatusCode;

                using (var httpRequestMessage =
                    new HttpRequestMessage(new HttpMethod(settings.Method), settings.Uri))
                {
                    if (settings.Headers != null)
                    {
                        foreach (var header in settings.Headers)
                        {
                            var secret = await _variableReplacer.ReplaceAsync(header.Value, context, cancellationToken, ACTION_PROCESS_HTTP);
                            if (string.IsNullOrEmpty(secret))
                            {
                                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                            }
                            else
                            {
                                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, secret);
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(settings.Body))
                    {
                        string contentType = null;
                        var body = await _variableReplacer.ReplaceAsync(settings.Body, context, cancellationToken, ACTION_PROCESS_HTTP);

                            settings.Headers?.TryGetValue("Content-Type", out contentType);
                            httpRequestMessage.Content = new StringContent(body, Encoding.UTF8,
                                contentType ?? "application/json");
                    }

                    if (CheckInternalUris(settings.Uri.AbsoluteUri))
                    {
                        AddHeadersToCommandRequest(httpRequestMessage, settings.currentStateId, context.OwnerIdentity);
                    }
                    else
                    {
                        AddBotIdentityToHeaders(httpRequestMessage, context);
                    }

                    AddUserToHeaders(httpRequestMessage, context);

                    using (var cts = new CancellationTokenSource(settings.RequestTimeout ?? DefaultRequestTimeout))
                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                    using (var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, linkedCts.Token).ConfigureAwait(false))
                    {
                        responseStatus = (int)httpResponseMessage.StatusCode;
                        isSuccessStatusCode = httpResponseMessage.IsSuccessStatusCode;

                        if (!string.IsNullOrWhiteSpace(settings.ResponseBodyVariable))
                        {
                            responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                        }
                    }
                }

                //Set the responses variables
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

                if (!isSuccessStatusCode)
                {
                    PushStatusCodeWarning(context, responseStatus);
                }
            }
            catch (HttpRequestException ex)
            {
                BuildHttpErrorMessage(settings, out responseStatus, out responseBody, ex);

                if (!string.IsNullOrWhiteSpace(settings.ResponseStatusVariable))
                {
                    await context.SetVariableAsync(settings.ResponseStatusVariable,
                        responseStatus.ToString(), cancellationToken);
                }

                if (!string.IsNullOrWhiteSpace(settings.ResponseBodyVariable))
                {
                    await context.SetVariableAsync(settings.ResponseBodyVariable,
                        responseBody, cancellationToken);
                }

            }
            catch (Exception ex)
            {
                _logger.Warning(ex, $"An exception occurred while processing HTTP action");
                if (ex is TaskCanceledException)
                {
                    PushTimeoutWarning(context);
                }
            }
            finally
            {
                SanitizeHeaders(context);
            }

        }

        /// <summary>
        /// Method to build a detailed error message for HttpRequestException based on errors in SSL/TLS
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="responseStatus"></param>
        /// <param name="responseBody"></param>
        /// <param name="ex"></param>
        private void BuildHttpErrorMessage(ProcessHttpSettings settings, out int responseStatus, out string responseBody, HttpRequestException ex)
        {
            var errorDetails = new StringBuilder();
            errorDetails.AppendLine($"Error: {ex.Message}");

            var isSslError = false;
            var isUntrustedRoot = false;

            if (ex.InnerException != null)
            {
                errorDetails.AppendLine($"Inner Exception: {ex.InnerException.Message}");

                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    if (innerEx is AuthenticationException)
                    {
                        isSslError = true;
                        errorDetails.AppendLine("SSL/TLS authentication error detected");
                    }

                    var exMessage = innerEx.Message.ToLowerInvariant();
                    if (exMessage.Contains("certificate") ||
                        exMessage.Contains("ssl") ||
                        exMessage.Contains("tls"))
                    {
                        isSslError = true;

                        if (exMessage.Contains("untrusted") ||
                            exMessage.Contains("trust") ||
                            exMessage.Contains("chain"))
                        {
                            isUntrustedRoot = true;
                            errorDetails.AppendLine("Certificate trust chain validation failed (Untrusted Root)");
                        }
                    }

                    innerEx = innerEx.InnerException;
                }
            }

            _logger.Error(ex, "HTTP request failed for URL: {Uri}. Details: {Details}",
                settings.Uri, errorDetails.ToString());


            var errorResponse = new
            {
                error = true,
                message = ex.Message,
                url = settings.Uri.ToString(),
                sslError = isSslError,
                untrustedRoot = isUntrustedRoot,
                details = errorDetails.ToString()
            };


            // Use a generic service unavailable status code (503) for non-SSL errors. 495 indicates ssl error.
            responseStatus = isSslError ? 495 : 503;
            responseBody = JsonConvert.SerializeObject(errorResponse);
        }

        private void PushTimeoutWarning(IContext context)
        {
            const string warningMessage = "The process http command action has timed out.";

            var currentActionTrace = context.GetCurrentActionTrace();
            if (currentActionTrace != null)
            {
                currentActionTrace.Warning = warningMessage;
            }
        }

        private void PushStatusCodeWarning(IContext context, int statusCode)
        {
            var warningMessage = $"Process http command action code response: {statusCode}";

            var currentActionTrace = context.GetCurrentActionTrace();
            if (currentActionTrace != null)
            {
                currentActionTrace.Warning = warningMessage;
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
            if (context.Flow.ConfigurationFlagIsEnabled(ADD_USER_KEY))
            {
                httpRequestMessage.Headers.Add(Constants.BLIP_USER_HEADER, context.UserIdentity);
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
            if (context.Flow.ConfigurationFlagIsEnabled(ADD_BOT_KEY))
            {
                httpRequestMessage.Headers.Add(Constants.BLIP_BOT_HEADER, context.OwnerIdentity);
            }
        }

        private void AddHeadersToCommandRequest(HttpRequestMessage httpRequestMessage, string currentStateId, string ownerIdentity)
        {
            httpRequestMessage.Headers.Add(Constants.BLIP_BOT_HEADER, ownerIdentity);
            httpRequestMessage.Headers.Add(Constants.BLIP_STATEID_HEADER, currentStateId);
        }

        private bool CheckInternalUris(string absoluteUri)
        {
            if (string.IsNullOrEmpty(_configuration.InternalUris))
            {
                return false;
            }
            var uriList = _configuration.InternalUris.Split(";");

            return uriList.Any((uri) => absoluteUri.Contains(uri));
        }

        private void SanitizeHeaders(IContext context)
        {
            if (context.Flow.ConfigurationFlagIsEnabled(SEND_HEADERS_TO_TRACE_COLLECTOR))
            {
                return;
            }

            var currentActionTrace = context.GetCurrentActionTrace();

            if (currentActionTrace != null)
            {
                var parsedSettings = currentActionTrace.ParsedSettings?.ToString(Formatting.None);
                var sanitizedSettings = _sensitiveInfoReplacer.ReplaceCredentials(parsedSettings);

                currentActionTrace.ParsedSettings = new JRaw(sanitizedSettings);

            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
