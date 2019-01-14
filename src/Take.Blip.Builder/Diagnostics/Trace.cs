using Lime.Protocol;
using System;

namespace Take.Blip.Builder.Diagnostics
{
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

        public DateTimeOffset Timestamp { get; set; }

        public long ElapsedMilliseconds { get; set; }

        public string Error { get; set; }
    }
}