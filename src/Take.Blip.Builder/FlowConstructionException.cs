using System;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines an exception thrown by the botmaker's flow construction.
    /// </summary>
    public class FlowConstructionException : BuilderException
    {
        /// <inheritdoc />
        public FlowConstructionException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public FlowConstructionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}