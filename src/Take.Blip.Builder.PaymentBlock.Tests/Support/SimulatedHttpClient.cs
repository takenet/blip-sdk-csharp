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

namespace Take.Blip.Builder.PaymentBlock.Tests.Support
{
    /// <summary>
    /// <see cref="IHttpClient"/> simulado do <c>request.fetchAsync</c>: responde em memória pela checkout-auth-api
    /// (<c>POST /bot</c>) e pela checkout-api (<c>POST /v0.1/checkout/purchase</c>,
    /// <c>GET /v0.1/checkout/customer/last-card</c> e <c>GET /v0.1/payment</c>) e grava cada chamada na sequência do
    /// <see cref="BuilderEngineHarness.InteractionLog"/>. Nenhuma chamada sai para a rede.
    /// </summary>
    public sealed class SimulatedHttpClient : IHttpClient
    {
        /// <summary>Caminho da emissão de token na checkout-auth-api.</summary>
        public const string AUTH_TOKEN_PATH = "/bot";

        /// <summary>Caminho da criação do link de pagamento na checkout-api.</summary>
        public const string PURCHASE_PATH = "/v0.1/checkout/purchase";

        /// <summary>Caminho da consulta do último cartão salvo na checkout-api.</summary>
        public const string LAST_CARD_PATH = "/v0.1/checkout/customer/last-card";

        /// <summary>Caminho da consulta do pagamento na checkout-api (o script do feedback chama <c>/V0.1/payment?id=</c>).</summary>
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

        /// <summary>Cria o cliente simulado gravando na sequência informada.</summary>
        public SimulatedHttpClient(BuilderEngineHarness.InteractionLog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            BotToken = SyntheticData.NewRunSecret();
        }

        /// <summary>Comportamento da checkout-api simulada.</summary>
        public enum CheckoutApiMode
        {
            /// <summary>Responde na hora, com sucesso.</summary>
            Normal,

            /// <summary>Responde 500 na criação do link; as outras rotas seguem normais.</summary>
            Error,

            /// <summary>Toda rota da checkout-api espera <see cref="CheckoutApiDelay"/> (cancelável) antes de responder.</summary>
            Delay,
        }

        /// <summary>Token devolvido pela checkout-auth-api simulada, gerado nesta execução.</summary>
        public string BotToken { get; }

        /// <summary>Segredo que a checkout-auth-api aceita; outro valor recebe 401.</summary>
        public string AcceptedSecret { get; set; }

        /// <summary>Comportamento atual da checkout-api simulada.</summary>
        public CheckoutApiMode CheckoutApi { get; set; } = CheckoutApiMode.Normal;

        /// <summary>Espera da checkout-api no modo <see cref="CheckoutApiMode.Delay"/>.</summary>
        public TimeSpan CheckoutApiDelay { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>Se a consulta do último cartão devolve um cartão salvo (200) ou nenhum (404).</summary>
        public bool HasSavedCard { get; set; }

        /// <summary>Meio informado pela consulta do pagamento, ou <c>null</c> se ela responde 404.</summary>
        public string ReportedPaymentMethod { get; private set; }

        /// <summary>Status informado pela consulta do pagamento, ou <c>null</c> se ela responde 404.</summary>
        public string ReportedPaymentStatus { get; private set; }

        /// <summary>
        /// Faz a consulta do pagamento da compra simulada informar <paramref name="paymentMethod"/> e <paramref name="status"/>
        /// com os nomes que a checkout-api devolve (<c>PaymentMethodsType</c> e <c>PurchaseStatus</c> como texto). Aceita
        /// qualquer caixa, porque os cenários escrevem o meio como o negócio (<c>PIX</c>) e a API responde <c>Pix</c>.
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

            throw new ArgumentException("Valor desconhecido pela checkout-api: " + value, parameterName);
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
