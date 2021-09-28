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

    }
}
