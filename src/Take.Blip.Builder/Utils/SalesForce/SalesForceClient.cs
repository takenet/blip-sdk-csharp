using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils.SalesForce.Models;

namespace Take.Blip.Builder.Utils.SalesForce
{
    public class SalesForceClient : ISalesForceClient
    {
        private readonly ILogger _logger;
        private const string LEAD_ATTRIBUTE_KEY = "attributes";

        public SalesForceClient(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<AuthorizationResponse> GetAuthorizationAsync(SalesForceConfig salesForceConfig, string ownerId, CancellationToken cancellationToken)
        {
            var body = new Dictionary<string, string>()
            {
                {"grant_type", "hybrid_refresh" },
                {SalesForceConstants.CLIENT_ID, salesForceConfig.ClientId },
                {SalesForceConstants.CLIENT_SECRET, salesForceConfig.ClientSecret},
                {SalesForceConstants.REFRESH_TOKEN, salesForceConfig.RefreshToken }
            };

            var responseToken = string.Empty;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.PostAsync(
                        $"{SalesForceConstants.SALES_FORCE_URI}{SalesForceRoutes.REFRESH_TOKEN}",
                        new FormUrlEncodedContent(body)
                        ).Result;
                    responseToken = await response.Content.ReadAsStringAsync();
                };
                return JsonConvert.DeserializeObject<AuthorizationResponse>(responseToken);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, $"An exception occurred while processing a Sales Force  HTTP request");
                throw ex;
            }
        }

        public async Task<LeadResponse> CreateLeadAsync(CrmSettings registerLeadSettings, AuthorizationResponse authorization)
        {
            var response = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        authorization.TokenType,
                        authorization.AccessToken
                        );
                    HttpResponseMessage responseMessage = client.PostAsync(
                        $"{authorization.InstanceUrl}{SalesForceRoutes.CREATE_LEAD}",
                        new StringContent(
                            registerLeadSettings.LeadBody as string,
                            Encoding.UTF8,
                            "application/json"
                            )
                        ).Result;
                    response = await responseMessage.Content.ReadAsStringAsync();
                };
                return JsonConvert.DeserializeObject<LeadResponse>(response);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, $"An exception occurred while processing a Sales Force  HTTP request");
                throw ex;
            }
        }

        public async Task<JObject> GetLeadAsync(CrmSettings settings, AuthorizationResponse authorization, CancellationToken cancellationToken)
        {
            var response = string.Empty;
            var uri = $"{authorization.InstanceUrl}" +
                $"{ApplyUriParams(SalesForceRoutes.GET_LEAD, settings.LeadId)}";
            
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        authorization.TokenType,
                        authorization.AccessToken
                        );
                    HttpResponseMessage responseMessage = client.GetAsync(
                        uri,
                        cancellationToken
                        ).Result;
                    response = await responseMessage.Content.ReadAsStringAsync();
                };
                var convertedObject = JsonConvert.DeserializeObject<JObject>(response);
                convertedObject.Remove(LEAD_ATTRIBUTE_KEY);
                return convertedObject;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, $"An exception occurred while processing a Sales Force  HTTP request");
                throw ex;
            }
        }

        private string ApplyUriParams(string uri, params string[] @params)
        {
            foreach (var param in @params.Select((value, i) => new { i, value }))
            {
                uri = uri.Replace($"{{{param.i}}}", param.value);
            }

            return uri;
        }
    }
}
