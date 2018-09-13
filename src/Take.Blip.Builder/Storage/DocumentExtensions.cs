using Lime.Protocol;
using Lime.Protocol.Serialization;

namespace Take.Blip.Builder.Storage
{
    public static class DocumentExtensions
    {
        public static StorageDocument ToStorageDocument(this Document document, IDocumentSerializer documentSerializer) => new StorageDocument
        {
            Document = documentSerializer.Serialize(document),
            Type = document.GetMediaType()
        };
    }
}
