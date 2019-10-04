using System;
using System.Threading;
using Lime.Protocol;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Allows delimiting a context where all calls should be done using the specified owner identity.
    /// </summary>
    public static class OwnerContext
    {
        private static readonly AsyncLocal<Identity> _owner = new AsyncLocal<Identity>();

        public static Identity Owner => _owner.Value;

        public static IDisposable Create(Identity owner)
        {
            // TODO: Create a stack to support multiple levels of contexts
            if (_owner.Value != null)
            {
                throw new InvalidOperationException("The owner is already defined for the current context");
            }

            _owner.Value = owner;
            return new ClearOwnerContext();        
        }
        
        private sealed class ClearOwnerContext : IDisposable
        {
            public void Dispose()
            {
                _owner.Value = null;
            }
        }
    }
}