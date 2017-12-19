using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public abstract class SenderActionBase
    {
        protected readonly ISender Sender;
        protected readonly JsonSerializer Serializer;

        protected SenderActionBase(ISender sender)
        {
            Sender = sender;
            Serializer = JsonSerializer.Create(JsonNetSerializer.Settings);
        }
    }
}