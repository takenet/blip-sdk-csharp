using Newtonsoft.Json;

namespace Take.Blip.Builder.Utils.SalesForce.Models
{
    /// <summary>
    /// Lead objec
    /// </summary>
    public class Lead
    {
        /// <summary>
        /// Lead Id
        /// </summary>
        [JsonProperty("Id")]
        public string Id { get; set; }

        /// <summary>
        /// First name property
        /// </summary>
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name property
        /// </summary>
        [JsonProperty("LastName")]
        public string LastName { get; set; }

        /// <summary>
        /// Email property
        /// </summary>
        [JsonProperty("Email")]
        public string Email { get; set; }

        /// <summary>
        /// Company property
        /// </summary>
        [JsonProperty("Company")]
        public string Company { get; set; }

        /// <summary>
        /// Variables attributes
        /// </summary>
        public JsonDictionaryAttribute Atributes { get; set; }

    }
}
