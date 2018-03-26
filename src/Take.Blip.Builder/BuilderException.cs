using System;
using System.Runtime.Serialization;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines an exception thrown by the builder engine.
    /// </summary>
    public class BuilderException : Exception
    {
        /// <inheritdoc />
        public BuilderException()
        {
        }

        /// <inheritdoc />
        public BuilderException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public BuilderException(string message, Exception innerException) : base(message, innerException)
        {
        }
        
        /// <inheritdoc />
        protected BuilderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}