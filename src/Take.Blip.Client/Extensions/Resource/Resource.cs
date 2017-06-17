using Lime.Protocol;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Takenet.Iris.Messaging.Contents
{
    /// <summary>
    /// Defines a reference to document resource.
    /// </summary>
    [DataContract]
    public class Resource : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.resource+json";
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        public Resource()
            : base(MediaType)
        {
        }

        /// <summary>
        /// The resource identifier.
        /// </summary>
        [DataMember(Name = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Resourcereplacement variables.
        /// </summary>
        [DataMember(Name = "variables")]
        public IDictionary<string, string> Variables { get; set; }
    }
}
