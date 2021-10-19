using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils.SalesForce.Models;

namespace Take.Blip.Builder.Utils.SalesForce
{
    /// <summary>
    /// Crm client
    /// </summary>
    public interface ICrmClient
    {
        /// <summary>
        /// Get crm authorization 
        /// </summary>
        /// <param name="crmConfig">sales force config object</param>
        /// <param name="ownerId">bot id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<AuthorizationResponse> GetAuthorizationAsync(CrmConfig crmConfig, string ownerId, CancellationToken cancellationToken);

        /// <summary>
        /// Create a lead on a given crm
        /// </summary>
        /// <param name="registerLeadSettings">register lead actions params</param>
        /// <param name="authorization">Authorization response object</param>
        /// <returns></returns>
        Task<LeadResponse> CreateLeadAsync(CrmSettings registerLeadSettings, AuthorizationResponse authorization);

        /// <summary>
        /// Get a specified lead for a given id
        /// </summary>
        /// <param name="settings">crm settings</param>
        /// <param name="authorization">authorization response object</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<JObject> GetLeadAsync(CrmSettings settings, AuthorizationResponse authorization, CancellationToken cancellationToken);
    }
}
