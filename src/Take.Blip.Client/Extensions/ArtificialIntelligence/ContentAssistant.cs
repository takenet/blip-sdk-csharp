using System;
using System.Runtime.Serialization;
using Lime.Protocol;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.Blip.Client.Extensions.ArtificialIntelligence
{
    //
    // Summary:
    //     Represents the match of combinations to a result.
    public class ContentAssistant : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.ai.content-assistant+json";

        public static readonly MediaType MediaType = MediaType.Parse("application/vnd.iris.ai.content-assistant+json");

        //
        // Summary:
        //     Name for easy content identification.
        [DataMember(Name = "name")]
        public string Name { get; set; }

        //
        // Summary:
        //     Guid content identification.
        [DataMember(Name = "guid")]
        public string Guid { get; set; }

        //
        // Summary:
        //     Content created date.
        [DataMember(Name = "storageDate")]
        public DateTimeOffset StorageDate { get; set; }

        //
        // Summary:
        //     The intent/entity combinations to filter for the result.
        [DataMember(Name = "combinations")]
        public ContentCombination[] Combinations { get; set; }

        //
        // Summary:
        //     The intent/entity combinations to filter for the result.
        [IgnoreDataMember]
        public string JsonValues { get; set; }

        //
        // Summary:
        //     The results of the content.
        [DataMember(Name = "results")]
        public Message[] Results { get; set; }

        public ContentAssistant()
            : base(MediaType)
        {
        }
    }
}