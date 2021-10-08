using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils;

namespace Take.Blip.Builder.Strategies
{
    /// <summary>
    /// Crm context strategy
    /// </summary>
    public interface ICrmContext
    {
        /// <summary>
        /// Set given crm
        /// </summary>
        /// <param name="crmProcessor">Crm processor class</param>
        void SetCrm(ICrmProcessor crmProcessor);

        /// <summary>
        /// Execute register lead on choosed crm
        /// </summary>
        /// <param name="context">bot context</param>
        /// <param name="settings">register lead settings</param>
        /// <param name="actionType">crm action type</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ExecuteAsync(IContext context, CrmSettings settings, ActionType actionType, CancellationToken cancellationToken);
    }
}
