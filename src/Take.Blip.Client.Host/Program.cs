using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Lime.Protocol.Server;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.Host
{
    class Program
    {
        const ConsoleColor HIGHLIGHT_COLOR = ConsoleColor.DarkCyan;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var optionsParserResult = Parser.Default.ParseArguments<Options>(args);
            
            if (optionsParserResult.Tag == ParserResultType.NotParsed)
            {
                var helpText = HelpText.AutoBuild(optionsParserResult);
                Console.Write(helpText);
                Console.Read();
                return;
            }
            var parsedOptions = (Parsed<Options>)optionsParserResult;
            await Run(parsedOptions.Value).ConfigureAwait(false);
        }

        private static async Task Run(Options options)
        {
            try
            {
                if (!File.Exists(options.ApplicationJsonPath))
                {
                    WriteLine($"Could not find the {options.ApplicationJsonPath} file", ConsoleColor.Red);
                    return;
                }

                WriteLine("Starting application...", HIGHLIGHT_COLOR);
                IStoppable stopabble;

                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(options.StartTimeout)))
                {
                    stopabble = await StartAsync(options.ApplicationJsonPath, cts.Token).ConfigureAwait(false);
                }

                WriteLine("Application started. Press any key to stop.", HIGHLIGHT_COLOR);
                Console.Read();
                WriteLine("Stopping application...", HIGHLIGHT_COLOR);
                await stopabble.StopAsync();
                WriteLine("Application stopped.", HIGHLIGHT_COLOR);
            }
            catch (OperationCanceledException)
            {
                WriteLine("Could not start the application in the configured timeout", ConsoleColor.Red);
            }
            catch (Exception ex)
            {
                WriteLine("Application failed:");
                WriteLine(ex.ToString(), ConsoleColor.Red);
            }
            finally
            {
                if (options.Pause)
                {
                    WriteLine("Press any key to exit.");
                    Console.ReadKey(true);
                }
            }
        }


        public static Task<IStoppable> StartAsync(string applicationFileName, CancellationToken cancellationToken)
        {
            //ConfigureWorkingDirectory(applicationFileName);
            var application = Application.ParseFromJsonFile(applicationFileName);
            return Bootstrapper.StartAsync(cancellationToken, application);
        }

        static void WriteLine(string value = "", ConsoleColor color = ConsoleColor.White)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = foregroundColor;
            Console.Out.Flush();
        }
    }
}