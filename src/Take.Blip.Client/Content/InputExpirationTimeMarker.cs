using Lime.Protocol;
using System.Runtime.Serialization;

namespace Take.Blip.Client.Content
{
    /// <summary>
    /// Represents an expired input with an user's <see cref="Identity"/> to make a fake input
    /// </summary>
    [DataContract]
    public class InputExpirationTimeMarker : Document
    {
        public const string MIME_TYPE = "application/vnd.blip-client.inputexpirationtime+json";

        /// <summary>
        /// Identity of the user whose input was expired. This identity will be used to make a fake input.
        /// </summary>
        [DataMember(Name = "identity")]
        public Identity Identity { get; set; }

        public InputExpirationTimeMarker(): base(MediaType.Parse(MIME_TYPE))
        { }
    }
}
