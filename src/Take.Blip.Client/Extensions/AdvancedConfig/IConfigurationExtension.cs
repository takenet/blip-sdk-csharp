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
        /// Get config by domain and key
        /// </summary>
        /// <typeparam name="T">Return object type</typeparam>
        /// <param name="domain">advanced config domain</param>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T> GetKeyValueAsync<T>(string domain, string key, CancellationToken cancellationToken);

        /// <summary>
        /// Set config 
        /// </summary>
        /// <param name="domain">config domain</param>
        /// <param name="resource">advanced config object</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SetConfigAsync(string domain, JsonDocument resource, CancellationToken cancellationToken);

    }
}
