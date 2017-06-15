using System.Runtime.Serialization;
using Lime.Protocol;

namespace Takenet.Iris.Messaging.Resources
{
    [DataContract]
    public class DistributionList : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.distribution-list+json";
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        public const string IDENTITY = "identity";

        public DistributionList()
            : base(MediaType)
        {
        }

        [DataMember(Name = IDENTITY)]
        public Identity Identity { get; set; }
    }
}
