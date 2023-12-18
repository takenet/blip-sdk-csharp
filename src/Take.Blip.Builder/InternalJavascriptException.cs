using System;
using Esprima;

namespace Take.Blip.Builder
{

    /// <summary>
    /// Class created to replace Jint.RuntimeJavascriptException using less properties
    /// </summary>
    public class InternalJavaScriptException : Exception
    {

        /// <summary>
        /// Location
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Callstack
        /// </summary>
        public string CallStack { get; set; }

        /// <summary>
        /// inheritdoc
        /// </summary>
        /// <param name="message"></param>
        public InternalJavaScriptException(string message) : base(message) { }
    }
}
