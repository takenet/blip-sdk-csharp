using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Directory
{
    /// <summary>
    /// Provides a service for querying user informations in the public account directory.
    /// </summary>
    public interface IDirectoryExtension
    {
        /// <summary>
        /// Gets an account information from the directory.
        /// </summary>
        /// <param name="identity">The identity to query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Account> GetDirectoryAccountAsync(Identity identity, CancellationToken cancellationToken);
    }
}
