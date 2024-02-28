using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Control a counter that limits expiration input executions
    /// </summary>
    public interface IInputExpirationCount
    {
        /// <summary>
        /// Remove to the counter.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<bool> TryRemoveAsync(Message message);
        /// <summary>
        /// Add to the counter.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<long> IncrementAsync(Message message);
    }
}
