using Lime.Protocol;

namespace Take.Blip.Builder
{
    internal class BucketIdHelper
    {
        public const string ID_PREFIX = "builder";
        public const string PRIVATE_ID_PREFIX = "!!";

        public static string GetId(string flowId, Identity user, string name) => $"{ID_PREFIX}:{flowId}:{user}:{name}";

        public static string GetPrivateId(string flowId, Identity user, string name) => GetId(flowId, user, $"{PRIVATE_ID_PREFIX}{name}");
    }
}