using Serilog;

namespace Take.Blip.Client
{
    public static class LoggerProvider
    {
        private static readonly object _syncRoot = new object();

        public static ILogger Logger
        {
            get
            {
                if (Log.Logger == null)
                {
                    lock (_syncRoot)
                    {
                        if (Log.Logger == null)
                        {
                            Log.Logger = new LoggerConfiguration().WriteTo.Trace().CreateLogger();
                        }
                    }
                }

                return Log.Logger;
            }
        }
    }
}
