using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils.SalesForce.Models;

namespace Take.Blip.Builder.Utils.SalesForce
{
    public class SalesForceClient : ICrmClient
    {
        private readonly ILogger _logger;
        private const string LEAD_ATTRIBUTE_KEY = "attributes";
        private readonly IConfiguration _configuration;

        public SalesForceClient(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<AuthorizationResponse> GetAuthorizationAsync(CrmConfig crmConfig, string ownerId, CancellationToken cancellationToken)
        {
            var salesForceConfig = crmConfig.SalesForceConfig;
            var body = new Dictionary<string, string>()
            {
                {SalesForceConstants.GRANT_TYPE_KEY, SalesForceConstants.GRANT_TYPE },
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
                        $"{_configuration.SalesForceBaseUri}{SalesForceRoutes.REFRESH_TOKEN}",
                        new FormUrlEncodedContent(body)
                        ).Result;
                    responseToken = await response.Content.ReadAsStringAsync();
                };
                return JsonConvert.DeserializeObject<AuthorizationResponse>(responseToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"An exception occurred while processing a Sales Force HTTP request");
                throw;
            }
        }

        public async Task<LeadResponse> CreateLeadAsync(CrmSettings registerLeadSettings, AuthorizationResponse authorization)
        {
            var response = string.Empty;
            var statusCode = HttpStatusCode.OK;

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
                    statusCode = responseMessage.StatusCode;
                    response = await responseMessage.Content.ReadAsStringAsync();
                };
                if (statusCode == HttpStatusCode.BadRequest)
                {
                    var convertedErrorResponse = JsonConvert.DeserializeObject<List<LeadResponse>>(response);
                    return convertedErrorResponse.First();
                }

                return JsonConvert.DeserializeObject<LeadResponse>(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"An exception occurred while processing a Sales Force HTTP request");
                throw;
            }
        }

        public async Task<JObject> GetLeadAsync(CrmSettings settings, AuthorizationResponse authorization, CancellationToken cancellationToken)
        {
            var response = string.Empty;
            var statusCode = HttpStatusCode.OK;

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
                    statusCode = responseMessage.StatusCode;
                    response = await responseMessage.Content.ReadAsStringAsync();
                };

                JObject convertedObject;
                if(statusCode == HttpStatusCode.NotFound)
                {
                    var notFoundResponse = JsonConvert.DeserializeObject<List<JObject>>(response);
                    convertedObject = notFoundResponse.First();
                }
                else
                {
                    convertedObject = JsonConvert.DeserializeObject<JObject>(response);
                    convertedObject.Remove(LEAD_ATTRIBUTE_KEY);
                }
                return convertedObject;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"An exception occurred while processing a Sales Force  HTTP request");
                throw;
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
