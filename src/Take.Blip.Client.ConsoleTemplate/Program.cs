using System;
using Take.Blip.Client.ConsoleHost;

namespace Take.Blip.Client.ConsoleTemplate
{
    class Program
    {
        static int Main(string[] args)
        {
            return ConsoleRunner.RunAsync(args).GetAwaiter().GetResult();
        }
    }
}