using System;
using Take.Blip.Client.ConsoleHost;

namespace $rootnamespace$
{
    class Program
    {
        static int Main(string[] args)
        {
            return ConsoleRunner.RunAsync(args).GetAwaiter().GetResult();
        }
    }
}