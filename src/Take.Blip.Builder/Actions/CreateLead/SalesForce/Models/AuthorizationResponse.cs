using Newtonsoft.Json;

namespace Take.Blip.Builder.Actions.CreateLead.SalesForce.Models
{
    /// <summary>
    /// Sales force authorization response
    /// </summary>
    public class AuthorizationResponse
    {
        /// <summary>
        /// Access token
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Sales force instance url
        /// </summary>
        [JsonProperty("instance_url")]
        public string InstanceUrl { get; set; }

        /// <summary>
        /// Sales orce token type
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
