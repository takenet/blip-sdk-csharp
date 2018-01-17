using System;
using System.Threading;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Stores a request context to be accessible statically.
    /// </summary>
    internal static class RequestContext
    {
        private static readonly AsyncLocal<Flow> _flow = new AsyncLocal<Flow>();
        private static readonly AsyncLocal<LazyInput> _input = new AsyncLocal<LazyInput>();

        /// <summary>
        /// Gets or sets the current input in the asynchronous context.
        /// </summary>
        public static LazyInput Input => _input.Value;

        /// <summary>
        /// Gets or sets the current flow in the asynchronous context.
        /// </summary>
        public static Flow Flow => _flow.Value;

        /// <summary>
        /// Registers a new request context.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="flow"></param>
        /// <returns></returns>
        public static IDisposable Create(LazyInput input, Flow flow)
        {
            // TODO: Create a stack to support multiple levels of contexts
            _input.Value = input;
            _flow.Value = flow;
            return new ClearInputContext();
        }

        private sealed class ClearInputContext : IDisposable
        {
            public void Dispose()
            {
                _input.Value = null;
                _flow.Value = null;
            }
        }
    }
}