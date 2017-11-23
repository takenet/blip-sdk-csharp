using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Context
{
    /// <summary>
    /// Defines a service for storing documents in an identity context.
    /// </summary>
    public interface IContextExtension
    {
        /// <summary>
        /// Gets an existing variable from the identity context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="variableName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> GetVariableAsync<T>(Identity identity, string variableName, CancellationToken cancellationToken) where T : Document;

        /// <summary>
        /// Set a documents into an identity context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="variableName"></param>
        /// <param name="document"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        Task SetVariableAsync<T>(Identity identity, string variableName, T document, CancellationToken cancellationToken, TimeSpan expiration = default(TimeSpan)) where T : Document;

        /// <summary>
        /// Set a documents into the global context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variableName"></param>
        /// <param name="document"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        Task SetGlobalVariableAsync<T>(string variableName, T document, CancellationToken cancellationToken, TimeSpan expiration = default(TimeSpan)) where T : Document;

        /// <summary>
        /// Set a documents from an identity context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="variableName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteVariableAsync(Identity identity, string variableName, CancellationToken cancellationToken);

        /// <summary>
        /// Set a documents from the global context.
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="variableName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteGlobalVariableAsync(string variableName, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the variables names of an identity context.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DocumentCollection> GetVariablesAsync(Identity identity, int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets the identities that have stored contexts.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DocumentCollection> GetIdentitiesAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken));
    }

    public static class ContextExtensionExtensions
    {
        /// <summary>
        /// Gets an existing text variable from the identity context.
        /// </summary>
        /// <param name="contextExtension"></param>
        /// <param name="identity"></param>
        /// <param name="variableName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> GetTextVariableAsync(this IContextExtension contextExtension, Identity identity, string variableName, CancellationToken cancellationToken)
        {
            var document = await contextExtension.GetVariableAsync<PlainText>(identity, variableName, cancellationToken);
            return document?.Text;
        }

        /// <summary>
        /// Set a text as value for an identity variable.
        /// </summary>
        /// <param name="contextExtension"></param>
        /// <param name="identity"></param>
        /// <param name="variableName"></param>
        /// <param name="variableValue"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public static Task SetTextVariableAsync(this IContextExtension contextExtension, Identity identity, string variableName, string variableValue, CancellationToken cancellationToken, TimeSpan expiration = default(TimeSpan)) 
            => contextExtension.SetVariableAsync(identity, variableName, new PlainText() {Text = variableValue }, cancellationToken, expiration);

        /// <summary>
        /// Set a text as value for an global variable.
        /// </summary>
        /// <param name="contextExtension"></param>
        /// <param name="variableName"></param>
        /// <param name="variableValue"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public static Task SetGlobalTextVariableAsync(this IContextExtension contextExtension, string variableName, string variableValue, CancellationToken cancellationToken, TimeSpan expiration = default(TimeSpan))
            => contextExtension.SetGlobalVariableAsync(variableName, new PlainText() { Text = variableName }, cancellationToken, expiration);
    }
}
