using Lime.Protocol;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Extensions.AdvancedConfig
{
    /// <summary>
    /// Advanced config extension
    /// </summary>
    public interface IConfigurationExtension
    {
        /// <summary>
        /// Get config by domain
        /// </summary>
        /// <param name="domain">advanced config domain</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Document> GetDomainAsync(string domain, CancellationToken cancellationToken);

        /// <summary>
        /// Get config by domain and key
        /// </summary>
        /// <param name="domain">advanced config domain</param>
        /// <param name="key">advanced config key</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetKeyValueAsync(string domain, string key, CancellationToken cancellationToken);

        /// <summary>
        /// Set config 
        /// </summary>
        /// <param name="domain">config domain</param>
        /// <param name="key">config key</param>
        /// <param name="value">advanced config value object</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetConfigAsync(string domain, string key, object value, CancellationToken cancellationToken);
    }
}
