using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Microsoft.ClearScript;
using Newtonsoft.Json;
using Serilog;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2.Functions
{
    /// <summary>
    /// Add HTTP request functions to the script engine.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Request
    {
        private readonly CancellationToken _cancellationToken;
        private readonly ExecuteScriptV2Settings _settings;
        private readonly IHttpClient _httpClient;
        private readonly IContext _context;
        private readonly Time _time;
        private readonly ILogger _logger;
        private const string APPLICATION_JSON = "application/json";

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="httpClient"></param>
        /// <param name="context"></param>
        /// <param name="time"></param>
        /// <param name="logger"></param>
        /// <param name="cancellationToken"></param>
        public Request(ExecuteScriptV2Settings settings, IHttpClient httpClient, IContext context,
            Time time,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _httpClient = httpClient;
            _settings = settings;
            _context = context;
            _time = time;
            _logger = logger.ForContext("OwnerIdentity", context.OwnerIdentity)
                .ForContext("UserIdentity", context.UserIdentity);
        }

        /// <summary>
        /// Sets a variable in the context.
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponse> FetchAsync(string uri, IScriptObject options = default)
        {
            using var httpRequestMessage =
                new HttpRequestMessage(
                    new HttpMethod(options?.GetProperty("method").ToString() ?? "GET"), uri);

            var contentType = await _setHeadersAsync(options, httpRequestMessage);

            await _setBodyAsync(options, httpRequestMessage, contentType);

            if (_context.OwnerIdentity != null && !((string)_context.OwnerIdentity).IsNullOrEmpty())
            {
                httpRequestMessage.Headers.Add(Constants.BLIP_BOT_HEADER, _context.OwnerIdentity);
            }

            if (!_settings.currentStateId.IsNullOrEmpty())
            {
                httpRequestMessage.Headers.Add(Constants.BLIP_STATEID_HEADER,
                    _settings.currentStateId);
            }

            if (_context.UserIdentity != null && !((string)_context.UserIdentity).IsNullOrEmpty())
            {
                httpRequestMessage.Headers.Add(Constants.BLIP_USER_HEADER, _context.UserIdentity);
            }

            using var httpResponseMessage =
                await _httpClient.SendAsync(httpRequestMessage, _cancellationToken);

            var responseStatus = (int)httpResponseMessage.StatusCode;
            var isSuccessStatusCode = httpResponseMessage.IsSuccessStatusCode;

            var responseBody = "";

            try
            {
                responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error reading response content");

                var currentActionTrace = _context.GetCurrentActionTrace();
                if (currentActionTrace != null)
                {
                    currentActionTrace.Warning =
                        $"Request.fetchAsync: failed to read body: {ex.Message}";
                }
            }

            return new HttpResponse(
                responseStatus,
                isSuccessStatusCode,
                responseBody,
                httpResponseMessage.Headers.ToDictionary(
                    h => h.Key.ToLower(),
                    h => h.Value.ToArray()));
        }
        private bool _isOnlyJsonContentType(string contentType)
        {
            return contentType.Split(',').Length == 1 && contentType.IsContentType(APPLICATION_JSON);
        }

        private async Task _setBodyAsync(IScriptObject options,
            HttpRequestMessage httpRequestMessage,
            string contentType)
        {
            var body = options?.GetProperty("body");
            if (body != null)
            {
                var requestBody = await ScriptObjectConverter.ToStringAsync(body, _time, _cancellationToken);

                if (_isOnlyJsonContentType(contentType))
                {
                    httpRequestMessage.Content = new StringContent(requestBody, Encoding.UTF8, contentType ?? APPLICATION_JSON);
                }
                else
                {
                    var byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(requestBody));
                    byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType ?? APPLICATION_JSON);
                    httpRequestMessage.Content = byteArrayContent;
                }
            }
        }

        private async Task<string> _setHeadersAsync(IScriptObject options,
            HttpRequestMessage httpRequestMessage)
        {
            string contentType = APPLICATION_JSON;
            if (!(options?.GetProperty("headers") is IScriptObject headers))
            {
                return contentType;
            }

            foreach (var header in headers.PropertyNames)
            {
                var headerValue = await ScriptObjectConverter.ConvertAsync(
                    headers.GetProperty(header), _time,
                    _cancellationToken);

                switch (headerValue)
                {
                    case string value:
                        if (header.Equals("content-type",
                                StringComparison.CurrentCultureIgnoreCase))
                        {
                            contentType = value;
                        }

                        httpRequestMessage.Headers.TryAddWithoutValidation(header, value);
                        break;
                    case List<object> values:
                        if (header.Equals("content-type",
                                StringComparison.CurrentCultureIgnoreCase) &&
                            values.Count > 0)
                        {
                            contentType = values[0].ToString();
                        }

                        httpRequestMessage.Headers.TryAddWithoutValidation(header,
                            values.Select(v => v.ToString()).ToArray());
                        break;
                }
            }

            return contentType;
        }

        /// <summary>
        /// The representation of the HTTP Response.
        /// </summary>
        public sealed class HttpResponse
        {
            /// <summary>
            /// Gets the response body as a JSON object.
            /// </summary>
            [JsonProperty("status")]
            public int Status { get; set; }

            /// <summary>
            /// Gets the response body as a JSON object.
            /// </summary>
            [JsonProperty("success")]
            public bool Success { get; set; }

            /// <summary>
            /// Gets the response body as a JSON object.
            /// </summary>
            [JsonProperty("body")]
            public string Body { get; set; }

            /// <summary>
            /// Gets the response body as a JSON object.
            /// </summary>
            [JsonProperty("headers")]
            public IDictionary<string, string[]> Headers { get; set; }

            /// <summary>
            /// The representation of the HTTP Response.
            /// </summary>
            /// <param name="status"></param>
            /// <param name="success"></param>
            /// <param name="body"></param>
            /// <param name="headers"></param>
            public HttpResponse(int status, bool success, string body,
                Dictionary<string, string[]> headers)
            {
                Status = status;
                Success = success;
                Body = body;
                Headers = headers;
            }

            /// <summary>
            /// Gets the response body as a JSON object.
            /// </summary>
            /// <returns></returns>
            public Task<ScriptObject> JsonAsync()
            {
                return Task.FromResult(ScriptEngine.Current.Script.JSON.parse(Body));
            }

            /// <summary>
            /// Gets the response body as a JSON object.
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public string[] GetHeader(string key)
            {
                return Headers.TryGetValue(key.ToLower(), out var value)
                    ? value
#pragma warning disable S1168 - Return null to diferentiate from empty array
                    : null;
#pragma warning restore S1168
            }
        }
    }
}