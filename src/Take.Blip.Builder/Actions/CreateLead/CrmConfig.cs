using Newtonsoft.Json;
using Take.Blip.Builder.Actions.CreateLead.SalesForce.Models;

namespace Take.Blip.Builder.Actions.CreateLead
{
    /// <summary>
    /// Crm's configuration
    /// </summary>
    public class CrmConfig
    {
        /// <summary>
        /// Sales force config
        /// </summary>
        [JsonProperty("SalesForce")]
        public SalesForceConfig SalesForceConfig { get; set; }

        /// <summary>
        /// Equals method override 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if(obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var cc = (CrmConfig)obj;

            return this.SalesForceConfig.ClientId.Equals(cc.SalesForceConfig.ClientId) &&
                this.SalesForceConfig.ClientSecret.Equals(cc.SalesForceConfig.ClientSecret) &&
                this.SalesForceConfig.RefreshToken.Equals(cc.SalesForceConfig.RefreshToken);
        }

        /// <summary>
        /// Get hash code override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => SalesForceConfig.GetHashCode();

    }
}
