using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Take.Blip.Client.Content
{
    /// <summary>
    /// Represents a expired input with a <see cref="Identity"/> of user to make a fake input
    /// </summary>
    [DataContract]
    public class InputExpirationTimeDocument : Document
    {
        public const string MIME_TYPE = "application/vnd.blip-client.inputexpirationtime+json";

        /// <summary>
        /// Identity of user that input was expired. This identity will be used to make a fake input.
        /// </summary>
        [DataMember(Name = "identity")]
        public Identity Identity { get; set; }

        public InputExpirationTimeDocument(): base(MediaType.Parse(MIME_TYPE))
        { }
    }
}
