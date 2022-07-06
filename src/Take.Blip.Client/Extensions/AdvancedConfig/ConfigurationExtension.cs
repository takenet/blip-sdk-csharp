using Lime.Protocol;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Extensions.AdvancedConfig
{
    public class ConfigurationExtension : IConfigurationExtension
    {
        private readonly ISender _sender;

        public ConfigurationExtension(ISender sender)
        {
            _sender = sender;
        }

        public async Task<Document> GetDomainAsync(string domain, CancellationToken cancellationToken)
        {
            var requestCommand = new Command()
            {
                Method = CommandMethod.Get,
                Uri = CreateLimeUri(domain)
            };

            var commandResponse = await _sender.ProcessCommandAsync(requestCommand, cancellationToken);
            return commandResponse.Resource;
        }

        public async Task<string> GetKeyValueAsync(string domain, string key, CancellationToken cancellationToken)
        {
            if (domain.IsNullOrEmpty()) throw new ArgumentNullException(nameof(domain));
            if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(key));

            var configDomain = (await GetDomainAsync(domain, cancellationToken)) as JsonDocument;
            return configDomain[key] as string;
        }

        public async Task<T> GetKeyValueAsync<T>(string domain, string key, CancellationToken cancellationToken)
        {
            var stringfiedResult = await GetKeyValueAsync(domain, key, cancellationToken);
            return JsonConvert.DeserializeObject<T>(stringfiedResult);
        }

        public async Task SetConfigAsync(string domain, JsonDocument resource, CancellationToken cancellationToken)
        {
            if (domain.IsNullOrEmpty()) throw new ArgumentNullException(nameof(domain));

            var requestCommand = new Command()
            {
                Method = CommandMethod.Set,
                Uri = CreateLimeUri(domain),
                Resource = resource as JsonDocument
            };

            await _sender.ProcessCommandAsync(requestCommand, cancellationToken);
        }

        private string CreateLimeUri(string domain)
        {
            return new LimeUri($"lime://{domain}/configuration");
        }
    }
}
