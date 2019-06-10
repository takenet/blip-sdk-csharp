namespace bot_flash_cards_blip_sdk_csharp
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Lime.Protocol.Server;
    using Serilog;
    using Serilog.Core;
    using Take.Blip.Client;

    /// <summary>
    /// Defines a type that is called once during the application initialization.
    /// </summary>
    public class Startup : IStartable
    {
        private readonly ISender _sender;
        private readonly Settings _settings;
        
        public Startup(ISender sender, Settings settings)
        {
            _sender = sender;
            _settings = settings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Trace().CreateLogger();

            return Task.CompletedTask;
        }
    }
}
