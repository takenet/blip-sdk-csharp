using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a flow loader service, used to load flow information when occors a redirect
    /// </summary>
    public interface IFlowLoader
    {
        /// <summary>
        /// Load a flow instance referring to an application 
        /// </summary>
        /// <param name="flowType">Flow type</param>
        /// <param name="parentFlow">Parent flow</param>
        /// <param name="identifier">Application identifier</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns></returns>
        Task<Flow> LoadFlowAsync(FlowType flowType, Flow parentFlow, string identifier, CancellationToken cancellationToken);
    }
}
