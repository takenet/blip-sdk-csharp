using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.UnitTests.PaymentBlock.Support
{
    /// <summary>
    /// Simulated <see cref="IHttpClient"/> for <c>request.fetchAsync</c>: answers in memory for the checkout-auth-api
    /// (<c>POST /bot</c>) and the checkout-api (<c>POST /v0.1/checkout/purchase</c>,
    /// <c>GET /v0.1/checkout/customer/last-card</c> and <c>GET /v0.1/payment</c>) and records each call in the sequence of the
    /// <see cref="BuilderEngineHarness.InteractionLog"/>. No call goes out to the network.
    /// </summary>
    public sealed class SimulatedHttpClient : IHttpClient
    {
        /// <summary>Path of the token issuing in the checkout-auth-api.</summary>
        public const string AUTH_TOKEN_PATH = "/bot";

        /// <summary>Path of the payment link creation in the checkout-api.</summary>
        public const string PURCHASE_PATH = "/v0.1/checkout/purchase";

        /// <summary>Path of the last saved card query in the checkout-api.</summary>
        public const string LAST_CARD_PATH = "/v0.1/checkout/customer/last-card";

        /// <summary>Path of the payment query in the checkout-api (the feedback script calls <c>/V0.1/payment?id=</c>).</summary>
        public const string PAYMENT_PATH = "/v0.1/payment";

        private const string AUTHORIZATION_HEADER = "Authorization";
        private const string BEARER_PREFIX = "Bearer ";
        private const string JSON_MEDIA_TYPE = "application/json";
        private const string PIX_PAYMENT_METHOD = "Pix";
        private const string PAYMENT_QUERY = "?id=" + SyntheticData.PURCHASE_ID;

        private static readonly string[] PAYMENT_METHODS = { "None", "CreditCard", PIX_PAYMENT_METHOD, "BolePix" };

        private static readonly string[] PAYMENT_STATUSES =
        {
            "None", "Created", "Processing", "Pending", "Denied", "Cancelled", "Completed", "Expired", "Failed", "Succeeded", "GenericError",
        };

        private readonly BuilderEngineHarness.InteractionLog _log;

        /// <summary>Creates the simulated client recording to the given sequence.</summary>
        public SimulatedHttpClient(BuilderEngineHarness.InteractionLog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            BotToken = SyntheticData.NewRunSecret();
        }

        /// <summary>Behavior of the simulated checkout-api.</summary>
        public enum CheckoutApiMode
        {
            /// <summary>Responds immediately, with success.</summary>
            Normal,

            /// <summary>Responds 500 on link creation; the other routes stay normal.</summary>
            Error,

            /// <summary>Every checkout-api route waits <see cref="CheckoutApiDelay"/> (cancellable) before responding.</summary>
            Delay,
        }

        /// <summary>Token returned by the simulated checkout-auth-api, generated in this run.</summary>
        public string BotToken { get; }

        /// <summary>Secret the checkout-auth-api accepts; any other value gets 401.</summary>
        public string AcceptedSecret { get; set; }

        /// <summary>Current behavior of the simulated checkout-api.</summary>
        public CheckoutApiMode CheckoutApi { get; set; } = CheckoutApiMode.Normal;

        /// <summary>checkout-api wait in <see cref="CheckoutApiMode.Delay"/> mode.</summary>
        public TimeSpan CheckoutApiDelay { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>Whether the last saved card query returns a saved card (200) or none (404).</summary>
        public bool HasSavedCard { get; set; }

        /// <summary>Payment method reported by the payment query, or <c>null</c> if it responds 404.</summary>
        public string ReportedPaymentMethod { get; private set; }

        /// <summary>Status reported by the payment query, or <c>null</c> if it responds 404.</summary>
        public string ReportedPaymentStatus { get; private set; }

        /// <summary>
        /// Makes the payment query of the simulated purchase report <paramref name="paymentMethod"/> and <paramref name="status"/>
        /// with the names the checkout-api returns (<c>PaymentMethodsType</c> and <c>PurchaseStatus</c> as text). Accepts
        /// any casing, because the tests write the method as the business does (<c>PIX</c>) and the API responds <c>Pix</c>.
        /// </summary>
        public void ReportPayment(string paymentMethod, string status)
        {
            ReportedPaymentMethod = ApiName(PAYMENT_METHODS, paymentMethod, nameof(paymentMethod));
            ReportedPaymentStatus = ApiName(PAYMENT_STATUSES, status, nameof(status));
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request?.RequestUri == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            var interaction = _log.Add(BuilderEngineHarness.InteractionKind.Http);
            interaction.HttpMethod = request.Method.Method;
            interaction.HttpUri = request.RequestUri;
            interaction.HttpRequestHeaders = CopyHeaders(request);
            interaction.HttpRequestBody = body;

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var (status, responseBody) = await RouteAsync(request, body, cancellationToken);
                interaction.HttpStatus = status;
                interaction.HttpResponseBody = responseBody;
                interaction.HttpElapsed = stopwatch.Elapsed;
                return new HttpResponseMessage((HttpStatusCode)status)
                {
                    Content = new StringContent(responseBody, Encoding.UTF8, JSON_MEDIA_TYPE),
                };
            }
            catch (OperationCanceledException)
            {
                interaction.HttpCancelled = true;
                interaction.HttpElapsed = stopwatch.Elapsed;
                throw;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        private async Task<(int Status, string Body)> RouteAsync(HttpRequestMessage request, string body, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri;
            if (string.Equals(uri.Host, SyntheticData.AUTH_HOST, StringComparison.OrdinalIgnoreCase))
            {
                return IsRoute(request, HttpMethod.Post, AUTH_TOKEN_PATH) ? IssueToken(body) : NotFound();
            }

            if (!string.Equals(uri.Host, SyntheticData.CHECKOUT_HOST, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            if (CheckoutApi == CheckoutApiMode.Delay)
            {
                await Task.Delay(CheckoutApiDelay, cancellationToken);
            }

            if (IsRoute(request, HttpMethod.Post, PURCHASE_PATH))
            {
                return CreatePurchase(request, body);
            }

            if (IsRoute(request, HttpMethod.Get, PAYMENT_PATH))
            {
                return GetPayment(request);
            }

            return IsRoute(request, HttpMethod.Get, LAST_CARD_PATH) ? GetLastCard(request) : NotFound();
        }

        private (int Status, string Body) IssueToken(string body)
        {
            var secret = (string)ParseObject(body)?["secret"];
            if (AcceptedSecret == null || !string.Equals(secret, AcceptedSecret, StringComparison.Ordinal))
            {
                return (401, Json(new JObject { ["message"] = "invalid credentials" }));
            }

            return (200, Json(new JObject { ["token"] = BotToken }));
        }

        private (int Status, string Body) CreatePurchase(HttpRequestMessage request, string body)
        {
            if (!IsAuthorized(request))
            {
                return Unauthorized();
            }

            if (CheckoutApi == CheckoutApiMode.Error)
            {
                return (500, Json(new JObject { ["message"] = "simulated failure creating the payment link" }));
            }

            var response = new JObject
            {
                ["purchaseId"] = SyntheticData.PURCHASE_ID,
                ["link"] = SyntheticData.PAYMENT_LINK,
            };

            if (RequestsPix(ParseObject(body)))
            {
                response["pix"] = new JObject
                {
                    ["qrCodeText"] = SyntheticData.PIX_COPY_CODE,
                    ["pixKey"] = SyntheticData.PIX_KEY_CNPJ,
                    ["merchantName"] = SyntheticData.PIX_MERCHANT_NAME,
                };
            }

            return (200, Json(response));
        }

        private (int Status, string Body) GetLastCard(HttpRequestMessage request)
        {
            if (!IsAuthorized(request))
            {
                return Unauthorized();
            }

            if (!HasSavedCard)
            {
                return NotFound();
            }

            return (200, Json(new JObject
            {
                ["id"] = SyntheticData.SAVED_CARD_ID,
                ["lastFourDigits"] = SyntheticData.SAVED_CARD_LAST_FOUR,
            }));
        }

        private (int Status, string Body) GetPayment(HttpRequestMessage request)
        {
            if (!IsAuthorized(request))
            {
                return Unauthorized();
            }

            if (ReportedPaymentStatus == null
                || !string.Equals(request.RequestUri.Query, PAYMENT_QUERY, StringComparison.Ordinal))
            {
                return NotFound();
            }

            return (200, Json(new JObject
            {
                ["id"] = SyntheticData.PURCHASE_ID,
                ["status"] = ReportedPaymentStatus,
                ["paymentMethod"] = ReportedPaymentMethod,
            }));
        }

        private static string ApiName(string[] names, string value, string parameterName)
        {
            foreach (var name in names)
            {
                if (string.Equals(name, value, StringComparison.OrdinalIgnoreCase))
                {
                    return name;
                }
            }

            throw new ArgumentException("Value unknown to checkout-api: " + value, parameterName);
        }

        private bool IsAuthorized(HttpRequestMessage request)
        {
            if (!request.Headers.TryGetValues(AUTHORIZATION_HEADER, out var values))
            {
                return false;
            }

            var expected = BEARER_PREFIX + BotToken;
            foreach (var value in values)
            {
                if (string.Equals(value, expected, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool RequestsPix(JObject body)
        {
            if (!(body?["paymentMethodsToGenerate"] is JArray methods))
            {
                return false;
            }

            foreach (var method in methods)
            {
                if (string.Equals((string)method, PIX_PAYMENT_METHOD, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsRoute(HttpRequestMessage request, HttpMethod method, string path)
            => request.Method == method
               && string.Equals(request.RequestUri.AbsolutePath, path, StringComparison.OrdinalIgnoreCase);

        private static IReadOnlyDictionary<string, string> CopyHeaders(HttpRequestMessage request)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in request.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }

            return headers;
        }

        private static JObject ParseObject(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                return JToken.Parse(body) as JObject;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        private static (int Status, string Body) Unauthorized()
            => (401, Json(new JObject { ["message"] = "unauthorized" }));

        private static (int Status, string Body) NotFound()
            => (404, Json(new JObject { ["message"] = "not found" }));

        private static string Json(JObject value) => value.ToString(Formatting.None);
    }
}
