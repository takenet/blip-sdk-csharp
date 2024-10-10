using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ProcessHttp
{
    /// <summary>
    /// Represents the settings required for processing an HTTP request.
    /// </summary>
    public class ProcessHttpSettings : IValidable
    {
        /// <summary>
        /// Gets or sets the HTTP method (e.g., GET, POST).
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the URI for the HTTP request.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Gets or sets the headers for the HTTP request.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets or sets the body of the HTTP request.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the variable to store the response body.
        /// </summary>
        public string ResponseBodyVariable { get; set; }

        /// <summary>
        /// Gets or sets the variable to store the response status.
        /// </summary>
        public string ResponseStatusVariable { get; set; }

        /// <summary>
        /// Gets or sets the timeout for the HTTP request.
        /// </summary>
        public TimeSpan? RequestTimeout { get; set; }

        /// <summary>
        /// Gets or sets the current state ID.
        /// </summary>
        public string currentStateId { get; set; }

        /// <summary>
        /// Gets or sets the time-to-live for the HTTP process.
        /// </summary>
        public TimeSpan ProccesHttpTimeToLive { get; set; }

        /// <summary>
        /// Validates the settings to ensure required values are provided.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown when a required setting value is missing.
        /// </exception>
        public void Validate()
        {
            if (Uri == null)
            {
                throw new ValidationException(
                    $"The '{nameof(Uri)}' settings value is required for '{nameof(ProcessHttpAction)}' action");
            }

            if (Method == null)
            {
                throw new ValidationException(
                    $"The '{nameof(Method)}' settings value is required for '{nameof(ProcessHttpAction)}' action");
            }
        }
    }
}