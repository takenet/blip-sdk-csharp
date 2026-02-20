using Lime.Protocol;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Take.Blip.Builder.Diagnostics
{
    /// <summary>
    /// Base class for monitoring traces.
    /// </summary>
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

        /// <summary>
        /// TimeStamp of when the trace was created.
        /// </summary>
        [DataMember(Name = "timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Elapsed time in milliseconds for the operation represented by the trace.
        /// </summary>
        [DataMember(Name = "elapsedMilliseconds")]
        public long ElapsedMilliseconds { get; set; }

        /// <summary>
        /// Warning message associated with the trace, if any.
        /// </summary>
        [DataMember(Name = "warning")]
        public string Warning { get; set; }

        /// <summary>
        /// Error message associated with the trace, if any.
        /// </summary>
        [DataMember(Name = "error")]
        public string Error { get; set; }
    }
}