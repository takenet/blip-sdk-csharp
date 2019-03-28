using System.Threading;

namespace Take.Blip.Builder.Utils
{
    public static class ContextContainer
    {
        private static AsyncLocal<IContext> _currentContext = new AsyncLocal<IContext>();

        /// <summary>
        /// Gets or sets the current context related to the message processing flow.
        /// </summary>
        /// <value>
        /// The current context.
        /// </value>
        public static IContext CurrentContext
        {
            get { return _currentContext.Value; }
            set { _currentContext.Value = value; }
        }
    }
}