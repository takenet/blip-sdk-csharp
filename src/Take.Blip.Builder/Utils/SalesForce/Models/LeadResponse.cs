using Newtonsoft.Json;
using System.Collections.Generic;

namespace Take.Blip.Builder.Utils.SalesForce.Models
{
    /// <summary>
    /// Lead creation response object
    /// </summary>
    public class LeadResponse
    {
        /// <summary>
        /// Lead id
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Bool that indicates if lead was created
        /// </summary>
        [JsonProperty("success")]
        public bool Succes { get; set; }

        /// <summary>
        ///List of errors
        /// </summary>
        [JsonProperty("errors")]
        public List<string> Errors { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
