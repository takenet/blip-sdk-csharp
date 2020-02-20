using Lime.Protocol;
using System;
using System.Runtime.Serialization;

namespace Take.Blip.Builder.Diagnostics
{
    [DataContract]
    public class Trace : Document
    {
        public static readonly MediaType MediaType = MediaType.Parse("application/vnd.blip.trace+json");

        public Trace() : base(MediaType)
        {
            Timestamp = DateTimeOffset.UtcNow;
        }

        public Trace(MediaType mediaType) : base(mediaType)
        {
        }

        [DataMember(Name = "timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [DataMember(Name = "elapsedMilliseconds")]
        public long ElapsedMilliseconds { get; set; }

        [DataMember(Name = "warning")]
        public string Warning { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }
    }
}