using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.CreateLead
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
        /// <param name="settings">register lead action settings</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task RegisterLead(IContext context, RegisterLeadSettings settings, CancellationToken cancellationToken);
    }
}
