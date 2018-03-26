using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json;

namespace Take.Blip.Builder.Actions
{
    public static class LimeSerializerContainer
    {
        public static readonly JsonSerializer Serializer = JsonSerializer.Create(JsonNetSerializer.Settings);
    }
}