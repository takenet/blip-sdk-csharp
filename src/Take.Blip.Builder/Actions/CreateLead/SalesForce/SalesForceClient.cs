using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.CreateLead.SalesForce.Models;

namespace Take.Blip.Builder.Actions.CreateLead.SalesForce
{
    public class SalesForceClient : ISalesForceClient
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public SalesForceClient(ILogger logger, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }
        public async Task<AuthorizationResponse> GetAuthorizationAsync(SalesForceConfig salesForceConfig, string ownerId)
        {
            var responseToken = string.Empty;
            var body = new Dictionary<string, string>()
            {
                {"grant_type", "hybrid_refresh" },
                {SalesForceConstants.CLIENT_ID, salesForceConfig.ClientId },
                {SalesForceConstants.CLIENT_SECRET, salesForceConfig.ClientSecret},
                {SalesForceConstants.REFRESH_TOKEN, salesForceConfig.RefreshToken }
            };

            return await _memoryCache.GetOrCreateAsync(ownerId, async cacheEntry =>
            {
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
                    cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(10);
                    return JsonConvert.DeserializeObject<AuthorizationResponse>(responseToken);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, $"An exception occurred while processing a Sales Force  HTTP request");
                    throw ex;
                }
            });
        }

        public async Task<LeadResponse> CreateLeadAsync(RegisterLeadSettings registerLeadSettings, AuthorizationResponse authorization)
        {
            var response = string.Empty;
            var body = JsonConvert.SerializeObject(registerLeadSettings.LeadBody);
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
                            body,
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
    }
}
