using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Session
{
    /// <summary>
    /// Defines a service that manages the states for filtering message and notification receivers registered in the application.
    /// </summary>
    public interface IStateManager
    {
        /// <summary>
        /// Gets or sets the state expiration timeout.
        /// </summary>
        /// <value>
        /// The state timeout.
        /// </value>
        TimeSpan StateTimeout { get; set; }

        /// <summary>
        /// Gets the last known node state.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        Task<string> GetStateAsync(Identity identity, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the node state.
        /// </summary>
        /// <param name="identity">The identity.</param>
        /// <param name="state">The state.</param>
        Task SetStateAsync(Identity identity, string state, CancellationToken cancellationToken);

        /// <summary>
        /// Resets the node state to the default value.
        /// </summary>
        /// <param name="identity">The identity.</param>
        Task ResetStateAsync(Identity identity, CancellationToken cancellationToken);

        /// <summary>
        /// Occurs when a state for a node is changed.
        /// </summary>
        event EventHandler<StateEventArgs> StateChanged;
    }


    /// <summary>
    /// Represents an event for the user state.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class StateEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateEventArgs"/> class.
        /// </summary>
        /// <param name="identity">The node.</param>
        /// <param name="state">The state.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public StateEventArgs(Identity identity, string state)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (state == null) throw new ArgumentNullException(nameof(state));
            Identity = identity;
            State = state;
        }

        /// <summary>
        /// Gets the identity.
        /// </summary>
        /// <value>
        /// The node.
        /// </value>
        public Identity Identity { get; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public string State { get; }
    }
}
