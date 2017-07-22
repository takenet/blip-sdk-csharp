using System;
using System.Collections.Generic;
using System.Text;
using DasMulli.Win32.ServiceUtils;

namespace Take.Blip.Client.ConsoleHost
{
    internal class BlipService : IWin32Service
    {
        public BlipService(Options options)
        {
            ServiceName = options.ServiceName;
        }

        public string ServiceName { get; }

        public void Start(string[] startupArguments, ServiceStoppedCallback serviceStoppedCallback)
        {            
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
