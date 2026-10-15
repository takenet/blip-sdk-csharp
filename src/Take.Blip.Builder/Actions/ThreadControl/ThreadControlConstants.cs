using System;
using System.Collections.Generic;
using Lime.Protocol;

namespace Take.Blip.Builder.Actions.ThreadControl
{
    internal static class ThreadControlConstants
    {
        // Confirm with Channels if gateway/domain changes.
        public static readonly Node GatewayAddress = Node.Parse("postmaster@wa.gw.msging.net");

        // Confirm with Channels if URI contracts change.
        public static readonly LimeUri PassUri = new LimeUri("/thread-control/pass");
        public static readonly LimeUri TakeUri = new LimeUri("/thread-control/take");

        public static readonly MediaType JsonMediaType = MediaType.Parse("application/json");

        public static readonly ISet<string> AllowedTargetRoles = new HashSet<string>(StringComparer.Ordinal)
        {
            "customer_service",
            "marketing",
            "utility",
            "ctwa",
            "ai_agent",
            "escalation"
        };
    }
}
