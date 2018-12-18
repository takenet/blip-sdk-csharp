using Lime.Messaging;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using Newtonsoft.Json;

namespace Take.Blip.Builder.Actions
{
    public static class LimeSerializerContainer
    {
        public static readonly IDocumentTypeResolver DocumentTypeResolver = new DocumentTypeResolver().WithMessagingDocuments();

        public static readonly EnvelopeSerializer EnvelopeSerializer = new EnvelopeSerializer(DocumentTypeResolver);

        public static readonly JsonSerializer Serializer = JsonSerializer.Create(EnvelopeSerializer.Settings);
    }
}