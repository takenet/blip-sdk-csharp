using System.Collections.Generic;
using Lime.Protocol;
using Newtonsoft.Json.Linq;

namespace Take.Blip.Builder.Actions.SetBucket
{
    public class SetBucketActionSettings
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public int? Expiration { get; set; }

        public string Document { get; set; }

        public Document ToDocument()
        {
            var mediaType = MediaType.Parse(Type);

            if (mediaType.IsJson)
            {
                var json = JObject.Parse(Document);
                return new JsonDocument(json.ToObject<Dictionary<string, object>>(), mediaType);
            }

            return new PlainDocument(Document, mediaType);
        }
    }
}