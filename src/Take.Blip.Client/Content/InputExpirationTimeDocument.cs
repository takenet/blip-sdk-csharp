using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Take.Blip.Client.Content
{
    [DataContract]
    public class InputExpirationTimeDocument : Document
    {
        public const string MIME_TYPE = "application/vnd.blip-client.inputexpirationtime+json";

        [DataMember(Name = "identity")]
        public Identity Identity { get; set; }

        public InputExpirationTimeDocument(): base(MediaType.Parse(MIME_TYPE))
        { }
    }
}
