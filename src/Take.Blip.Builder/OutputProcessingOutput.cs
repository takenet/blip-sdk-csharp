using System;
using System.Runtime.Serialization;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Represents an exception thrown during the processing of output conditions in the engine.
    /// </summary>
    public class OutputProcessingException : BuilderException
    {
        /// <inheritdoc />
        public OutputProcessingException()
        {
        }

        /// <inheritdoc />
        public OutputProcessingException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public OutputProcessingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        protected OutputProcessingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string StateId { get; set; }

        public Condition[] OutputConditions { get; set; }
    }
}
