using System.Collections.Generic;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions.ThreadControl
{
    internal static class ThreadControlCommandFactory
    {
        public static Command CreateSetCommand(LimeUri uri, JObject resource)
        {
            return new Command
            {
                Id = EnvelopeId.NewId(),
                Method = CommandMethod.Set,
                Type = ThreadControlConstants.JsonMediaType,
                To = ThreadControlConstants.GatewayAddress,
                Uri = uri,
                Resource = new JsonDocument(
                    resource?.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>(),
                    ThreadControlConstants.JsonMediaType)
            };
        }
    }
}
