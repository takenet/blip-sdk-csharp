using System;
using Take.Blip.Client.Console;

namespace Take.Blip.Client.ConsoleHostTemplate
{
    class Program
    {
        static int Main(string[] args) => ConsoleRunner.RunAsync(args).GetAwaiter().GetResult();
    }
}