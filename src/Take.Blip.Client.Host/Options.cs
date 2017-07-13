using CommandLine;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.Host
{
    public class Options
    {
        [Value(0, HelpText = "The path of the application host JSON file.", Default = Bootstrapper.DefaultApplicationFileName)]
        public string ApplicationJsonPath { get; set; }

        [Option(HelpText = "The timeout in seconds to wait for the startup of the application.", Default = 30)]
        public int StartTimeout { get; set; }

        [Option(HelpText = "Wait for a key input before exiting the process.", Default = false)]
        public bool Pause { get; set; }
    }
}
