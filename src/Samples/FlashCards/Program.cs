namespace bot_flash_cards_blip_sdk_csharp
{
    using System;
    using Take.Blip.Client.Console;
    
    class Program
    {
        static int Main(string[] args) => ConsoleRunner.RunAsync(args).GetAwaiter().GetResult();
    }
}