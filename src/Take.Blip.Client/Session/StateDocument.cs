using System.Runtime.Serialization;
using Lime.Protocol;

namespace Take.Blip.Client.Session
{
    [DataContract]
    public class StateDocument : Document
    {
        public static readonly MediaType MediaType = MediaType.Parse("application/vnd.takenet.state+json");

        public StateDocument() : base(MediaType)
        {
        }

        [DataMember(Name = "state")]
        public string State { get; set; }
    }
}
