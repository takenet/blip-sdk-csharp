using System;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines an exception thrown by the botmaker's flow construction.
    /// </summary>
    public class FlowConstructionException : BuilderException
    {
        private const string FLOW_CONSTRUCTION_TAG = "[FlowConstruction]";

        /// <inheritdoc />
        public FlowConstructionException(string message) : base(ModifyMessage(message))
        {
        }

        /// <inheritdoc />
        public FlowConstructionException(string message, Exception innerException) : base(ModifyMessage(message), innerException)
        {
        }

        private static string ModifyMessage(string message)
        {
            return $"{FLOW_CONSTRUCTION_TAG} {message}";
        }
    }
}