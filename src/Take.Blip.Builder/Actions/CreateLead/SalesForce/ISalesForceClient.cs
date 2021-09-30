using System.Threading.Tasks;
using Take.Blip.Builder.Actions.CreateLead.SalesForce.Models;

namespace Take.Blip.Builder.Actions.CreateLead.SalesForce
{
    /// <summary>
    /// Sales force client
    /// </summary>
    public interface ISalesForceClient
    {
        /// <summary>
        /// Get authorization from refresh token 
        /// </summary>
        /// <param name="salesForceConfig">sales force config object</param>
        /// <param name="ownerId">bot id</param>
        /// <returns></returns>
        public Task<AuthorizationResponse> GetAuthorizationAsync(SalesForceConfig salesForceConfig, string ownerId);

        /// <summary>
        /// Create a lead on sales force
        /// </summary>
        /// <param name="registerLeadSettings">register lead actions params</param>
        /// <param name="authorization">Authorization response object</param>
        /// <returns></returns>
        Task<LeadResponse> CreateLeadAsync(RegisterLeadSettings registerLeadSettings, AuthorizationResponse authorization);
    }
}
