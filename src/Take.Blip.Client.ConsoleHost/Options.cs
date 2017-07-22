using CommandLine;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.ConsoleHost
{
    /// <summary>
    /// Represents the command line options.
    /// </summary>
    public class Options
    {
        public const string RUN_AS_SERVICE_FLAG = "run-as-service";
        public const string INSTALL_FLAG = "install";
        public const string UNINSTALL_FLAG = "uninstall";

        [Value(0, HelpText = "The path of the application host JSON file.", Default = Bootstrapper.DefaultApplicationFileName)]
        public string ApplicationJsonPath { get; set; }

        [Option(HelpText = "The timeout in seconds to wait for the startup of the application.", Default = 30)]
        public int StartTimeout { get; set; }

        [Option(HelpText = "Wait for a key input before exiting the process.", Default = false)]
        public bool Pause { get; set; }

        [Option(HelpText = "Runs the application as a service (only on Windows)", Default = false, SetName = RUN_AS_SERVICE_FLAG)]
        public bool RunAsService { get; set; }

        [Option(HelpText = "Install the application as service (only on Windows)", Default = false, SetName = INSTALL_FLAG)]
        public bool Install { get; set; }

        [Option(HelpText = "Uninstall a previously installed service (only on Windows)", Default = false, SetName = UNINSTALL_FLAG)]
        public bool Uninstall { get; set; }

        [Option(HelpText = "The service name for installation or uninstallation")]
        public string ServiceName { get; set; }

        [Option(HelpText = "The service display name for installation")]
        public string ServiceDisplayName { get; set; }

        [Option(HelpText = "The service description for installation")]
        public string ServiceDescription { get; set; }
    }
}
