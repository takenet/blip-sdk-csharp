using System;
using System.Threading;
using Lime.Protocol;

namespace Take.Blip.Client
{
    /// <summary>
    /// Stores information about the envelope receiver that is currently being called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class EnvelopeReceiverContext<T> where T : Envelope
    {
        private static readonly AsyncLocal<T> _envelope = new AsyncLocal<T>();

        /// <summary>
        /// Gets the envelope that is currently being processed by the receiver.
        /// </summary>
        public static T Envelope => _envelope.Value;

        /// <summary>
        /// Creates a new context for the specified envelope type.
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static IDisposable Create(T envelope)
        {
            if (_envelope.Value != null)
            {
                throw new InvalidOperationException("The envelope is already defined");
            }
            
            // TODO: Create a stack to support multiple levels of contexts
            _envelope.Value = envelope;
            return new ClearEnvelopeReceiverContext();
        }

        private sealed class ClearEnvelopeReceiverContext : IDisposable
        {
            public void Dispose()
            {
                _envelope.Value = null;
            }
        }
    }
}
