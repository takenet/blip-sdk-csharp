using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CommandLine;
using DasMulli.Win32.ServiceUtils;
using Lime.Protocol.Server;

namespace Take.Blip.Client.ConsoleHost
{
    internal class BlipService : IWin32Service
    {
        private IStoppable _stoppable;

        public BlipService(string serviceName)
        {
            ServiceName = serviceName;
        }

        public string ServiceName { get; }

        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {
            var optionsParserResult = Parser.Default.ParseArguments<Options>(startupArguments);
            if (optionsParserResult.Tag == ParserResultType.NotParsed)
            {
                throw new ArgumentException("Invalid arguments");
            }
            var options = ((Parsed<Options>)optionsParserResult).Value;
            options.RunAsService = false;
            options.ServiceName = ServiceName;
            var applicationJsonPath = ConsoleRunner.GetApplicationJsonPath(options);
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(options.StartTimeout)))
            {
                _stoppable = ConsoleRunner.StartAsync(applicationJsonPath, cts.Token).Result;
            }
        }

        public void Stop()
        {
            _stoppable.StopAsync().Wait();
        }
    }
}
