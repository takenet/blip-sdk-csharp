using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Server;
using Take.Blip.Client;

namespace Take.Blip.Client.ConsoleTemplate
{
    public class Startup : IStartable
    {
        private readonly ISender _sender;
        private readonly IDictionary<string, object> _settings;

        public Startup(ISender sender, IDictionary<string, object> settings)
        {
            _sender = sender;
            _settings = settings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
