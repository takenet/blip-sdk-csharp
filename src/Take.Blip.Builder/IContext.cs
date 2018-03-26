using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a conversation context.
    /// </summary>
    public interface IContext
    {        
        /// <summary>
        /// The identity of the user in the conversation.
        /// </summary>
        Identity User { get; }

        /// <summary>
        /// The current input document of the user.
        /// </summary>
        LazyInput Input { get; }

        /// <summary>
        /// The flow that is being processed.
        /// </summary>
        Flow Flow { get; }

        /// <summary>
        /// Sets a context variable value for the user.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        Task SetVariableAsync(string name, string value, CancellationToken cancellationToken, TimeSpan expiration = default(TimeSpan));

        /// <summary>
        /// Gets a defined context variable value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetVariableAsync(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a variable value in the context.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteVariableAsync(string name, CancellationToken cancellationToken);
    }
}
