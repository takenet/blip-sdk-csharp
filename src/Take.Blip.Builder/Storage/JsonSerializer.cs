using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Take.Elephant;

namespace Take.Blip.Builder.Storage
{
    public class JsonSerializer<T> : ISerializer<T> where T : class
    {
        private readonly EnvelopeSerializer _envelopeSerializer;

        public JsonSerializer(IEnvelopeSerializer envelopeSerializer)
        {
            _envelopeSerializer = (envelopeSerializer as EnvelopeSerializer) ?? throw new ArgumentException("Unsupported envelope serializer type");
        }

        public string Serialize(T value)
        {
            return JsonConvert.SerializeObject(value, Formatting.None, _envelopeSerializer.Settings);
        }

        public T Deserialize(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, _envelopeSerializer.Settings);
        }
    }
}
