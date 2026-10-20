using Lime.Protocol;

namespace Take.Blip.Builder.Actions.ThreadControl
{
    internal static class ThreadControlCommandFactory
    {
        public static readonly Node gatewayAddress = Node.Parse("postmaster@wa.gw.msging.net");
        public static readonly LimeUri threadControlUri = new LimeUri("/thread-control");

        public static Command CreateSetCommand(ThreadControlCommandResource resource)
        {
            return new Command
            {
                Id = EnvelopeId.NewId(),
                Method = CommandMethod.Set,
                To = gatewayAddress,
                Uri = threadControlUri,
                Resource = resource
            };
        }
    }
}
