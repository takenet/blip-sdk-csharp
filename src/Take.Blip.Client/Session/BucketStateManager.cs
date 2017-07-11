using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client.Extensions.Bucket;

namespace Take.Blip.Client.Session
{
    /// <summary>
    /// Provides the management of states for filtering message and notification receivers registered in the application.
    /// </summary>
    public sealed class BucketStateManager : IStateManager
    {
        public const string StateSessionKey = "$stateManagerState$";

        private static readonly TimeSpan CommandTimeout = TimeSpan.FromMilliseconds(5000);

        private readonly IBucketExtension _bucket;

        public BucketStateManager(IBucketExtension bucket)
        {
            StateTimeout = TimeSpan.FromMinutes(30);
            _bucket = bucket;
        }

        /// <summary>
        /// Gets or sets the state expiration timeout.
        /// </summary>
        /// <value>
        /// The state timeout.
        /// </value>
        public TimeSpan StateTimeout { get; set; }

        /// <summary>
        /// Gets the last known identity state.
        /// </summary>
        /// <param name="identity">The node.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public async Task<string> GetStateAsync(Identity identity, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            using (var cts = new CancellationTokenSource(CommandTimeout))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
            {
                return (await _bucket.GetAsync<StateDocument>(GetKey(identity), linkedCts.Token))?.State ?? Constants.DEFAULT_STATE;
            }
        }

        /// <summary>
        /// Sets the identity state.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="state">The state.</param>
        public Task SetStateAsync(Identity identity, string state, CancellationToken cancellationToken)
        {
            return SetStateAsync(identity, state, true, cancellationToken);
        }

        /// <summary>
        /// Resets the identity state to the default value.
        /// </summary>
        /// <param name="identity">The node.</param>
        public Task ResetStateAsync(Identity identity, CancellationToken cancellationToken)
        {
            return SetStateAsync(identity, Constants.DEFAULT_STATE, cancellationToken);
        }

        /// <summary>
        /// Occurs when a identity state is changed.
        /// This event should be used to synchronize multiple application instances states.
        /// </summary>
        public event EventHandler<StateEventArgs> StateChanged;

        internal async Task SetStateAsync(Identity identity, string state, bool raiseEvent, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Equals(Constants.DEFAULT_STATE, StringComparison.OrdinalIgnoreCase))
            {
                using (var cts = new CancellationTokenSource(CommandTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                {
                    await _bucket.DeleteAsync(GetKey(identity), linkedCts.Token);
                }
            }
            else
            {
                using (var cts = new CancellationTokenSource(CommandTimeout))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
                {
                    await _bucket.SetAsync(GetKey(identity), new StateDocument { State = state }, StateTimeout, linkedCts.Token);
                }
            }

            if (raiseEvent)
            {
                StateChanged?.Invoke(this, new StateEventArgs(identity, state));
            }
        }

        private static string GetKey(Identity identity)
        {
            return $"{StateSessionKey}_{identity.Name.ToLowerInvariant()}@{identity.Domain.ToLowerInvariant()}";
        }
    }
}
