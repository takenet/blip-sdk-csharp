using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Lime.Protocol.Server;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.ConsoleHost
{
    /// <summary>
    /// Defines a service for running BLiP applications in a console environment.
    /// </summary>
    public static class ConsoleRunner
    {
        const ConsoleColor HIGHLIGHT_COLOR = ConsoleColor.DarkCyan;
    
        /// <summary>
        /// Runs an console application with the specified arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<int> RunAsync(string[] args)
        {
            var optionsParserResult = Parser.Default.ParseArguments<Options>(args);

            if (optionsParserResult.Tag == ParserResultType.NotParsed)
            {
                var helpText = HelpText.AutoBuild(optionsParserResult);
                Console.Write(helpText);
                return 0;
            }
            var parsedOptions = (Parsed<Options>)optionsParserResult;
            return await RunAsync(parsedOptions.Value).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs an console application with the specified options.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<int> RunAsync(Options options)
        {
            try
            {
                var applicationJsonPath = options.ApplicationJsonPath;

                if (string.IsNullOrWhiteSpace(Path.GetDirectoryName(applicationJsonPath)))
                {
                    applicationJsonPath = Path.GetFullPath(applicationJsonPath);
                }

                if (!File.Exists(applicationJsonPath))
                {
                    WriteLine($"Could not find the '{options.ApplicationJsonPath}' file in '{applicationJsonPath}' path.", ConsoleColor.Red);
                    return -1;
                }

                WriteLine("Starting application...", HIGHLIGHT_COLOR);
                IStoppable stopabble;

                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(options.StartTimeout)))
                {
                    stopabble = await StartAsync(applicationJsonPath, cts.Token).ConfigureAwait(false);
                }

                WriteLine("Application started. Press any key to stop.", HIGHLIGHT_COLOR);
                Console.Read();
                WriteLine("Stopping application...", HIGHLIGHT_COLOR);
                await stopabble.StopAsync();
                WriteLine("Application stopped.", HIGHLIGHT_COLOR);
                return 0;
            }
            catch (OperationCanceledException)
            {
                WriteLine("Could not start the application in the configured timeout", ConsoleColor.Red);
                return -1;
            }
            catch (Exception ex)
            {
                WriteLine("Application failed:");
                WriteLine(ex.ToString(), ConsoleColor.Red);
                return -1;
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

        private static Task<IStoppable> StartAsync(string applicationFileName, CancellationToken cancellationToken)
        {
            var application = Application.ParseFromJsonFile(applicationFileName);
            var workingDir = Path.GetDirectoryName(applicationFileName);
            if (string.IsNullOrWhiteSpace(workingDir)) workingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);


            return Bootstrapper.StartAsync(cancellationToken, application, typeResolver: new TypeResolver(workingDir));
        }

        private static void WriteLine(string value = "", ConsoleColor color = ConsoleColor.White)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = foregroundColor;
            Console.Out.Flush();
        }
    }
}