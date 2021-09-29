using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.CreateLead.SalesForce.Models;
using Take.Blip.Client.Extensions.AdvancedConfig;

namespace Take.Blip.Builder.Actions.CreateLead.SalesForce
{
    public class SalesForceProcessor : ICrmProcessor
    {
        private readonly ILogger _logger;
        private readonly IConfigurationExtension _configurationExtension;
        private readonly IMemoryCache _memoryCache;

        public SalesForceProcessor(ILogger logger, IConfigurationExtension configurationExtension, IMemoryCache memoryCache)
        {
            _logger = logger;
            _configurationExtension = configurationExtension;
            _memoryCache = memoryCache;
        }

        public async Task RegisterLead(IContext context, RegisterLeadSettings settings, CancellationToken cancellationToken)
        {
            var stringifiedConfig = await _configurationExtension.GetKeyValueAsync(
                SalesForceConstants.CONFIG_DOMAIN,
                SalesForceConstants.CRM_CONFIG_KEY,
                cancellationToken
                );

            var crmConfig = JsonConvert.DeserializeObject<CrmConfig>(stringifiedConfig);
            var salesForceAuth = await GetAuthorizationAsync(crmConfig.SalesForceConfig, context.OwnerIdentity);
            var leadResponse = await CreateLeadAsync(settings, salesForceAuth);
            await context.SetVariableAsync(settings.ReturnValue, leadResponse.ToString(), cancellationToken);


            throw new NotImplementedException();
        }

        private async Task<AuthorizationResponse> GetAuthorizationAsync(SalesForceConfig salesForceConfig, string ownerId)
        {
            var responseToken = string.Empty;
            var body = new Dictionary<string, string>()
            {
                {"grant_type", "hybrid_fresh" },
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

        private async Task<LeadResponse> CreateLeadAsync(RegisterLeadSettings registerLeadSettings, AuthorizationResponse authorization)
        {
            var response = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        $"{authorization.TokenType} {authorization.AccessToken}"
                        );
                    HttpResponseMessage responseMessage = client.PostAsync(
                        $"{authorization.InstanceUrl}{SalesForceRoutes.CREATE_LEAD}",
                        new StringContent(
                            registerLeadSettings.LeadBody.ToString(),
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
