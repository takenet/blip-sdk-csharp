using System;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a builder exception analyze service.
    /// </summary>
    public interface IAnalyzeBuilderExceptions
    {
        /// <summary>
        /// Process an exception and determine if is Flow Construction Exception or not.
        /// </summary>
        /// <param name="ex">An exception that occurred during the flow.</param>
        /// <returns></returns>
        Exception VerifyFlowConstructionException(Exception ex);

        /// <summary>
        /// Process a flow construction exception message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns></returns>
        string CreateFlowConstructionExceptionMessage(string message);
    }
}