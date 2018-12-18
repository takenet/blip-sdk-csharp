using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;

namespace Take.Blip.Builder.Storage
{
    [DataContract]
    public class StorageDocument
    {
        public const string TYPE_KEY = "$type";

        [DataMember]
        public string Document { get; set; }

        [DataMember]
        public MediaType Type { get; set; }

        [DataMember]
        public DateTimeOffset? Expiration { get; set; }

        public Document ToDocument(IDocumentSerializer documentSerializer, JsonSerializer serializer)
        {
            var mediaType = Type;
            if (mediaType == null)
            {
                // Use the old method to determine the document type
                var jObject = JObject.Parse(Document);
                mediaType = jObject[TYPE_KEY].ToObject<MediaType>(serializer);
            }

            return documentSerializer.Deserialize(Document, mediaType);
        }
    }
}
