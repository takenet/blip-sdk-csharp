using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Take.Blip.Builder.Diagnostics
{
    /// <summary>
    /// Represents a custom trace entry added explicitly by a flow builder during execution.
    /// Used by the <c>Trace</c> action to record application-level diagnostic information
    /// such as API responses, business checkpoints, or exception details.
    /// </summary>
    [DataContract]
    public class UserTraceEntry
    {
        /// <summary>
        /// Gets or sets the name that identifies this trace entry.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets an optional category used to group related trace entries.
        /// </summary>
        [DataMember(Name = "category")]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets an optional free-form value associated with this trace entry.
        /// Useful for recording a single scalar result without a full key-value dictionary.
        /// </summary>
        [DataMember(Name = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets arbitrary key-value pairs with additional diagnostic data.
        /// All values are resolved after variable replacement, so they reflect
        /// the runtime state at the moment the action executes.
        /// </summary>
        [DataMember(Name = "data")]
        public IDictionary<string, string> Data { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp at which this entry was recorded.
        /// </summary>
        [DataMember(Name = "timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }
}

