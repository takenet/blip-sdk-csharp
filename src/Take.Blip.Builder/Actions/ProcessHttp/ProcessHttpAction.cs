﻿using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Actions.ProcessHttp
{
    public sealed class ProcessHttpAction : ActionBase<ProcessHttpSettings>, IDisposable
    {
        private const string ADD_USER_KEY = "processHttpAddUserToRequestHeader";
        private const string ADD_BOT_KEY = "processHttpAddBotIdentityToRequestHeader";
        public static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(60);

        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public ProcessHttpAction(IHttpClient httpClient, ILogger logger, IConfiguration configuration)
            : base(nameof(ProcessHttp))
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
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

                if (responseStatus != 200)
                {
                    PushStatusCodeWarning(context);
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

        private void PushStatusCodeWarning(IContext context, int statusCode = 0)
        {
            try
            {
                const string warningMessage = "The process http command action has returned non-success status code.";

                var currentActionTrace = context.GetCurrentActionTrace();
                if (currentActionTrace == null)
                {
                    return;
                }

                currentActionTrace.Warning = warningMessage;

                if (currentActionTrace.ParsedSettings != null)
                {
                    currentActionTrace.ParsedSettings["#responseStatusCode"] = statusCode;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while pushing status code warning");
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

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
