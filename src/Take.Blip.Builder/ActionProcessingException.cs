using System;
using System.Runtime.Serialization;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Represents an exception thrown during the processing of actions in the engine.
    /// </summary>
    public class ActionProcessingException : BuilderException
    {
        /// <inheritdoc />
        public ActionProcessingException()
        {
        }

        /// <inheritdoc />
        public ActionProcessingException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public ActionProcessingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        protected ActionProcessingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
