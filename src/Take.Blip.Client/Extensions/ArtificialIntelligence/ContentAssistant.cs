using System;
using System.Runtime.Serialization;
using Lime.Protocol;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.Blip.Client.Extensions.ArtificialIntelligence
{
    /// <summary>
    /// Represents the match of combinations to a result.
    /// </summary>
    public class ContentAssistant : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.ai.content-assistant+json";

        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        /// <summary>
        /// Name for easy content identification.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Guid content identification.
        /// </summary>
        [DataMember(Name = "guid")]
        public string Guid { get; set; }

        /// <summary>
        /// Content created date.
        /// </summary>
        [DataMember(Name = "storageDate")]
        public DateTimeOffset StorageDate { get; set; }

        /// <summary>
        /// The intent/entity combinations to filter for the result.
        /// </summary>
        [DataMember(Name = "combinations")]
        public ContentCombination[] Combinations { get; set; }

        /// <summary>
        /// The intent/entity combinations to filter for the result.
        /// </summary>
        [IgnoreDataMember]
        public string JsonValues { get; set; }

        /// <summary>
        /// The results of the content.
        /// </summary>
        [DataMember(Name = "results")]
        public Result[] Results { get; set; }

        public ContentAssistant()
            : base(MediaType)
        {
        }
    }

    /// <summary>
    /// The result of the content.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Tags for easy content identification.
        /// </summary>
        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }

        /// <summary>
        /// Type content identification.
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        /// <summary>
        /// The content.
        /// </summary>
        [DataMember(Name = "content")]
        public string Content { get; set; }
    }
}