using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Utils
{
    /// <summary>
    /// Crm processor
    /// </summary>
    public interface ICrmProcessor
    {
        /// <summary>
        /// Create a lead in  the given crm
        /// </summary>
        /// <param name="context">bot context</param>
        /// <param name="settings">crm actions settings</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RegisterLeadAsync(IContext context, CrmSettings settings, CancellationToken cancellationToken);

        /// <summary>
        /// Get a lead 
        /// </summary>
        /// <param name="context">bot context</param>
        /// <param name="settings">crm settings</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task GetLeadAsync(IContext context, CrmSettings settings, CancellationToken cancellationToken);

    }
}
