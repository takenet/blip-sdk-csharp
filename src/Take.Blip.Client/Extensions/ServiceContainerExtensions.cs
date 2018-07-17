using Lime.Messaging;
using Lime.Protocol.Serialization;
using System;
using System.Reflection;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.AttendanceForwarding;
using Take.Blip.Client.Extensions.Broadcast;
using Take.Blip.Client.Extensions.Bucket;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.Context;
using Take.Blip.Client.Extensions.Delegation;
using Take.Blip.Client.Extensions.Directory;
using Take.Blip.Client.Extensions.EventTracker;
using Take.Blip.Client.Extensions.Profile;
using Take.Blip.Client.Extensions.Resource;
using Take.Blip.Client.Extensions.Scheduler;
using Take.Blip.Client.Extensions.Threads;
using Take.Blip.Client.Extensions.Tunnel;

namespace Take.Blip.Client.Extensions
{
    public static class ServiceContainerExtensions
    {
        internal static IServiceContainer RegisterExtensions(this IServiceContainer serviceContainer)
        {
            var documentTypeResolver = new DocumentTypeResolver().WithMessagingDocuments();
            documentTypeResolver.RegisterAssemblyDocuments(typeof(Takenet.Iris.Messaging.Contents.Attendance).Assembly);
            documentTypeResolver.RegisterAssemblyDocuments(typeof(ServiceContainerExtensions).Assembly);
            serviceContainer.RegisterService(typeof(IDocumentTypeResolver), documentTypeResolver);

            Func<ISender> senderFactory = () => serviceContainer.GetService<ISender>();
            serviceContainer.RegisterService(typeof(IBroadcastExtension), () => new BroadcastExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IDelegationExtension), () => new DelegationExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IDirectoryExtension), () => new DirectoryExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IContactExtension), () => new ContactExtension(senderFactory()));
            Func<IBucketExtension> bucketExtensionFactory = () => new BucketExtension(senderFactory());
            serviceContainer.RegisterService(typeof(ISchedulerExtension), () => new SchedulerExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IEventTrackExtension), () => new EventTrackExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IProfileExtension), () => new ProfileExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IBucketExtension), bucketExtensionFactory);
            serviceContainer.RegisterService(typeof(IAttendanceExtension), () => new AttendanceExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(ITalkServiceExtension), () => new TalkServiceExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IThreadExtension), () => new ThreadExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IResourceExtension), () => new ResourceExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(ITunnelExtension), () => new TunnelExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IArtificialIntelligenceExtension), () => new ArtificialIntelligenceExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IContextExtension), () => new ContextExtension(senderFactory()));

            return serviceContainer;
        }
    }
}
