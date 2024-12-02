using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Builder
{
    /// <summary>
    /// Commands to builder application.
    /// </summary>
    public interface IBuilderExtension
    {
        /// <summary>
        /// Retrieves the specified function from the Blip Function.
        /// </summary>
        /// <param name="nameFunction">The name of the function to retrieve.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The function code as a Document.</returns>
        Task<Document> GetFunctionOnBlipFunctionAsync(string nameFunction, CancellationToken cancellationToken);
    }
}
