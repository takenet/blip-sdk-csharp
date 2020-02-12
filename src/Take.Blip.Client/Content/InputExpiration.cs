using Lime.Protocol;
using System.Runtime.Serialization;

namespace Take.Blip.Client.Content
{
    /// <summary>
    /// Represents an expired input with an user's <see cref="Identity"/> to make a fake input
    /// </summary>
    [DataContract]
    public class InputExpiration : Document
    {
        public const string MIME_TYPE = "application/vnd.blip-client.inputexpirationtime+json";

        /// <summary>
        /// Identity of the user whose input was expired. This identity will be used to make a fake input.
        /// </summary>
        [DataMember(Name = "identity")]
        public Identity Identity { get; set; }

        /// <summary>
        /// stateId to identify if user change state while message was being sent.
        /// </summary>
        [DataMember(Name = "stateId")]
        public string StateId { get; set; }

        public InputExpiration(): base(MediaType.Parse(MIME_TYPE))
        { }
    }
}
