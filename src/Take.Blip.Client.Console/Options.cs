using CommandLine;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.Console
{
    /// <summary>
    /// Represents the command line options.
    /// </summary>
    public class Options
    {
        public const string START_TIMEOUT_FLAG = "start-timeout";
        public const string RUN_AS_SERVICE_FLAG = "run-as-service";
        public const string INSTALL_FLAG = "install";
        public const string UNINSTALL_FLAG = "uninstall";
        public const string SERVICE_NAME_FLAG = "service-name";
        public const string SERVICE_DISPLAY_NAME_FLAG = "service-display-name";
        public const string SERVICE_DESCRIPTION_FLAG = "service-description";

        [Value(0, HelpText = "The path of the application host JSON file.", Default = Bootstrapper.DefaultApplicationFileName)]
        public string ApplicationJsonPath { get; set; }

        [Option(START_TIMEOUT_FLAG, HelpText = "The timeout in seconds to wait for the startup of the application.", Default = 30)]
        public int StartTimeout { get; set; }

        [Option(HelpText = "Wait for a key input before exiting the process.", Default = false)]
        public bool Pause { get; set; }

        [Option(RUN_AS_SERVICE_FLAG, HelpText = "Runs the application as a service (only on Windows)", Default = false)]
        public bool RunAsService { get; set; }

        [Option(INSTALL_FLAG, HelpText = "Install the application as service (only on Windows)", Default = false)]
        public bool Install { get; set; }

        [Option(UNINSTALL_FLAG, HelpText = "Uninstall a previously installed service (only on Windows)", Default = false)]
        public bool Uninstall { get; set; }

        [Option(SERVICE_NAME_FLAG, HelpText = "The service name for installation or uninstallation")]
        public string ServiceName { get; set; }

        [Option(SERVICE_DISPLAY_NAME_FLAG, HelpText = "The service display name for installation")]
        public string ServiceDisplayName { get; set; }

        [Option(SERVICE_DESCRIPTION_FLAG, HelpText = "The service description for installation")]
        public string ServiceDescription { get; set; }
    }
}
