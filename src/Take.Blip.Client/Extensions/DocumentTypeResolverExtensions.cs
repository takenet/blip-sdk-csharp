using Lime.Messaging;
using Lime.Protocol.Serialization;
using Take.Blip.Client.Content;
using Takenet.Iris.Messaging.Contents;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;
using Takenet.Iris.Messaging.Resources.Desk;
using Takenet.Iris.Messaging.Resources.Media;
using Takenet.Iris.Messaging.Resources.Portal;

namespace Take.Blip.Client.Extensions
{
    public static class DocumentTypeResolverExtensions
    {
        public static IDocumentTypeResolver WithBlipDocuments(this IDocumentTypeResolver documentTypeResolver)
        {
            documentTypeResolver = documentTypeResolver.WithMessagingDocuments();
            documentTypeResolver.RegisterAssemblyDocuments(typeof(Attendance).Assembly);
            documentTypeResolver.RegisterAssemblyDocuments(typeof(Intention).Assembly);
            documentTypeResolver.RegisterAssemblyDocuments(typeof(Attendant).Assembly);
            documentTypeResolver.RegisterAssemblyDocuments(typeof(DetailMedia).Assembly);
            documentTypeResolver.RegisterAssemblyDocuments(typeof(Application).Assembly);
            documentTypeResolver.RegisterAssemblyDocuments(typeof(InputExpiration).Assembly);
            documentTypeResolver.RegisterAssemblyDocuments(typeof(DocumentTypeResolverExtensions).Assembly);
            return documentTypeResolver;
        }
    }
}