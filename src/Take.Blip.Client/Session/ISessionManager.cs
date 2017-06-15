using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Session
{
    /// <summary>
    /// Defines a session management service.
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Gets an existing session for the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<NavigationSession> GetSessionAsync(Node node, CancellationToken cancellationToken);

        /// <summary>
        /// Clears an existing session for a node, removing all associated variable and states.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task ClearSessionAsync(Node node, CancellationToken cancellationToken);

        /// <summary>
        /// Adds an variable to a node session. If the session doesn't exists, it will be created.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task AddVariableAsync(Node node, string key, string value, CancellationToken cancellationToken);

        /// <summary>
        /// Gets an existing variable from a node session.
        /// </summary>
        /// <param name="node">The session node.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<string> GetVariableAsync(Node node, string key, CancellationToken cancellationToken);

        /// <summary>
        /// Removes an existing variable from a node session.
        /// </summary>
        /// <param name="node">The session node.</param>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task RemoveVariableAsync(Node node, string key, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the node session culture. This value should be specified using the SetCultureAsync method.
        /// </summary>
        /// <param name="node">The session node.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetCultureAsync(Node node, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the node session culture.
        /// </summary>
        /// <param name="node">The session node.</param>
        /// <param name="culture">The culture code.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetCultureAsync(Node node, string culture, CancellationToken cancellationToken);
    }
}
