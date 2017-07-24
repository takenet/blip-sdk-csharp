using System;
using Take.Blip.Client.Console;

namespace Take.Blip.Client.ConsoleTemplate
{
    class Program
    {
        static int Main(string[] args) => ConsoleRunner.RunAsync(args).GetAwaiter().GetResult();
    }
}