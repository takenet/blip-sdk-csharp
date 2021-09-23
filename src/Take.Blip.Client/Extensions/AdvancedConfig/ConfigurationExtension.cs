using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Text;
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
            if (domain == null) throw new ArgumentNullException(nameof(domain));
            var requestCommand = new Command()
            {
                Method = CommandMethod.Get,
                Uri = CreateLimeUri(domain)
            };

            var commandResponse = await _sender.ProcessCommandAsync(requestCommand, cancellationToken);
            return commandResponse.Resource;
        }

        public async Task<JsonDocument> GeyKeyValueAsync(string domain, string key, CancellationToken cancellationToken)
        {
            if (domain == null) throw new ArgumentNullException(nameof(domain));
            var configDomain = (await GetDomainAsync(domain, cancellationToken)) as JsonDocument;
            return configDomain[key] as JsonDocument;
        }

        public async Task SetConfigAsync(string domain, string key, object value, CancellationToken cancellationToken)
        {
            if (domain == null) throw new ArgumentNullException(nameof(domain));

            var resource = new JsonDocument(
                new Dictionary<string, object>()
                {
                    { key, value}
                },
                MediaType.ApplicationJson
                );

            var requestCommand = new Command()
            {
                Method = CommandMethod.Set,
                Uri = CreateLimeUri(domain),
                Resource = resource
            };

            await _sender.ProcessCommandAsync(requestCommand, cancellationToken);
        }

        private string CreateLimeUri(string domain)
        {
            return new LimeUri($"lime://{domain}/configuration");
        }
    }
}
