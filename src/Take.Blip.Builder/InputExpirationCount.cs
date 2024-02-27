using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Storage;

namespace Take.Blip.Builder
{
    /// <inheritdoc/>
    public class InputExpirationCount : IInputExpirationCount
    {
        private readonly ConcurrentDictionary<FromToIdentityInputExpirationPair, int> _inputExpirationCounts;

        /// <summary>
        /// Constructor
        /// </summary>
        public InputExpirationCount()
        {
            _inputExpirationCounts = new ConcurrentDictionary<FromToIdentityInputExpirationPair, int>();
        }

        /// <inheritdoc/>
        public Task<int> IncrementAsync(Message message)
        {
            var key = GetKey(message);
            var inputExpirationCount = _inputExpirationCounts.AddOrUpdate(
                key,
                addValueFactory: _ => 1,
                updateValueFactory: (_, count) => count + 1
            );
            return Task.FromResult(inputExpirationCount);
        }

        /// <inheritdoc/>
        public Task<bool> TryRemoveAsync(Message message)
        {
            var key = GetKey(message);
            return Task.FromResult(_inputExpirationCounts.TryRemove(key, out _));
        }

        private FromToIdentityInputExpirationPair GetKey(Message message)
        {
            return new FromToIdentityInputExpirationPair()
            {
                FromIdentity = message.To.ToIdentity(),
                ToIdentity = message.From.ToIdentity()
            };
        }
    }
}