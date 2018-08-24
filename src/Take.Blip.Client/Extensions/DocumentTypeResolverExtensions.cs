using Lime.Messaging;
using Lime.Protocol.Serialization;

namespace Take.Blip.Client.Extensions
{
    public static class DocumentTypeResolverExtensions
    {
        public static IDocumentTypeResolver WithBlipDocuments(this IDocumentTypeResolver documentTypeResolver)
        {
            documentTypeResolver = documentTypeResolver.WithMessagingDocuments();
            documentTypeResolver.RegisterAssemblyDocuments(typeof(Takenet.Iris.Messaging.Contents.Attendance).Assembly);
            documentTypeResolver.RegisterAssemblyDocuments(typeof(DocumentTypeResolverExtensions).Assembly);
            return documentTypeResolver;
        }
    }
}