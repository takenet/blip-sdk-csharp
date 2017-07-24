using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using DasMulli.Win32.ServiceUtils;
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
        public static Task<int> RunAsync(string[] args)
        {
            var optionsParserResult = Parser.Default.ParseArguments<Options>(args);
            if (optionsParserResult.Tag == ParserResultType.NotParsed)
            {
                HelpText.AutoBuild(optionsParserResult);
                return Task.FromResult(0);
            }
            var parsedOptions = (Parsed<Options>)optionsParserResult;
            return RunAsync(parsedOptions.Value);
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
                if (options.RunAsService) return RunAsService(options);
                if (options.Install) return InstallService(options);
                if (options.Uninstall) return UninstallService(options);
            
                string applicationJsonPath = GetApplicationJsonPath(options);

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
                await stopabble.StopAsync().ConfigureAwait(false);
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

        internal static string GetApplicationJsonPath(Options options)
        {
            var applicationJsonPath = options.ApplicationJsonPath;

            if (string.IsNullOrWhiteSpace(Path.GetDirectoryName(applicationJsonPath)))
            {
                applicationJsonPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    applicationJsonPath);
            }

            return applicationJsonPath;
        }

        internal static Task<IStoppable> StartAsync(string applicationFileName, CancellationToken cancellationToken)
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

        private static int RunAsService(Options options)
        {
            if (string.IsNullOrWhiteSpace(options.ServiceName))
            {
                WriteLine("Service name is required for uninstalling", ConsoleColor.Red);
                return -1;
            }

            var service = new BlipService(options.ServiceName);
            var serviceHost = new Win32ServiceHost(service);
            return serviceHost.Run();
        }

        private static int InstallService(Options options)
        {
            if (string.IsNullOrWhiteSpace(options.ServiceName))
            {
                WriteLine("Service name is required for installing", ConsoleColor.Red);
                return -1;
            }

            options.ServiceDisplayName = options.ServiceDisplayName ?? options.ServiceName;

            var remainingArgs = Environment.GetCommandLineArgs()
                .Where(arg => arg != $"--{Options.INSTALL_FLAG}")
                .Select(EscapeCommandLineArgument)
                .ToList();

            remainingArgs.Add($"\"--{Options.RUN_AS_SERVICE_FLAG}\"");

            var host = Process.GetCurrentProcess().MainModule.FileName;

            if (!host.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase))
            {
                // For self-contained apps, skip the dll path
                remainingArgs = remainingArgs.Skip(1).ToList();
            }

            var fullServiceCommand = host + " " + string.Join(" ", remainingArgs);

            new Win32ServiceManager()
                .CreateService(
                    options.ServiceName,
                    options.ServiceDisplayName,
                    options.ServiceDescription,
                    fullServiceCommand,
                    Win32ServiceCredentials.LocalSystem,
                    autoStart: true,
                    errorSeverity: ErrorSeverity.Normal
                );

            WriteLine($@"Successfully registered service ""{options.ServiceDisplayName}""");
            return 0;
        }

        private static int UninstallService(Options options)
        {
            if (string.IsNullOrWhiteSpace(options.ServiceName))
            {
                WriteLine("Service name is required for uninstall", ConsoleColor.Red);
                return -1;
            }

            new Win32ServiceManager()
                .DeleteService(options.ServiceName);

            Console.WriteLine($@"Successfully unregistered service ""{options.ServiceDisplayName}""");
            return 0;
        }

        private static string EscapeCommandLineArgument(string arg)
        {
            // http://stackoverflow.com/a/6040946/784387
            arg = Regex.Replace(arg, @"(\\*)" + "\"", @"$1$1\" + "\"");
            arg = "\"" + Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
            return arg;
        }
    }
}