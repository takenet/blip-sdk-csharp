using Lime.Protocol;
using Lime.Protocol.Serialization;
using Takenet.Elephant;

namespace Take.Blip.Builder.Storage
{
    public class MessageSerializer : ISerializer<Message>
    {
        private readonly IEnvelopeSerializer _envelopeSerializer;

        public MessageSerializer(IEnvelopeSerializer envelopeSerializer)
        {
            _envelopeSerializer = envelopeSerializer;
        }
        public string Serialize(Message value) => _envelopeSerializer.Serialize(value);

        public Message Deserialize(string value) => (Message) _envelopeSerializer.Deserialize(value);
    }
}