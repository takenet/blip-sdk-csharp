using Newtonsoft.Json;

namespace Take.Blip.Builder.Utils.SalesForce.Models
{
    /// <summary>
    /// Sales force config object
    /// </summary>
    public class SalesForceConfig
    {
        /// <summary>
        /// Sales force refresh token
        /// </summary>
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Sales force clientId
        /// </summary>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Sales force clientSecret
        /// </summary>
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }
    }
}
