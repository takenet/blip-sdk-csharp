using System;
using System.Collections.Generic;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.Trace
{
    /// <summary>
    /// Settings for the <see cref="TraceAction"/>.
    /// All string fields are resolved after variable replacement, so they can reference
    /// flow variables using the standard <c>{{variableName}}</c> syntax.
    /// </summary>
    public class TraceActionSettings : IValidable
    {
        /// <summary>
        /// Gets or sets a name that identifies this trace entry within the flow execution.
        /// Required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets an optional category used to group related trace entries
        /// (e.g. <c>"api-response"</c>, <c>"exception"</c>, <c>"checkpoint"</c>).
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets an optional free-form scalar value to record with the entry.
        /// Useful for concise single-value traces without requiring a full <see cref="Data"/> dictionary.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets arbitrary key-value pairs carrying additional diagnostic data
        /// (e.g. response status codes, identifiers, computed results).
        /// </summary>
        public IDictionary<string, string> Data { get; set; }

        /// <inheritdoc />
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException(
                    "The 'Name' settings value is required for 'TraceAction'."
                );
        }
    }
}

