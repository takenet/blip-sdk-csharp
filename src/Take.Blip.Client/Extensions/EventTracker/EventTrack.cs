using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Lime.Protocol;

namespace Takenet.Iris.Messaging.Resources
{
    [DataContract]
    public class EventTrack : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.eventTrack+json";
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        /// <summary>
        /// Initializes a new instance of the <see cref="EventTrack"/> class.
        /// </summary>
        public EventTrack() :
            base(MediaType)
        {
        }


        [DataMember(Name = "ownerIdentity")]
        public Identity OwnerIdentity { get; set; }

        [DataMember(Name = "storageDate")]
        public DateTimeOffset StorageDate { get; set; }

        [DataMember(Name = "category")]
        public string Category { get; set; }

        [DataMember(Name = "action")]
        public string Action { get; set; }

        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "extras")]
        public IDictionary<string, string> Extras { get; set; }

        [DataMember(Name = "identity")]
        public Identity Identity { get; set; }
    }
}
