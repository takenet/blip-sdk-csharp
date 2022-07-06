﻿using Lime.Protocol.Serialization;
using System;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.AdvancedConfig;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.AttendanceForwarding;
using Take.Blip.Client.Extensions.Broadcast;
using Take.Blip.Client.Extensions.Bucket;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.ContactsJourney;
using Take.Blip.Client.Extensions.Context;
using Take.Blip.Client.Extensions.Delegation;
using Take.Blip.Client.Extensions.Directory;
using Take.Blip.Client.Extensions.EventTracker;
using Take.Blip.Client.Extensions.HelpDesk;
using Take.Blip.Client.Extensions.Profile;
using Take.Blip.Client.Extensions.Resource;
using Take.Blip.Client.Extensions.Scheduler;
using Take.Blip.Client.Extensions.Threads;
using Take.Blip.Client.Extensions.Tunnel;

namespace Take.Blip.Client.Extensions
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer RegisterExtensions(this IServiceContainer serviceContainer)
        {
            serviceContainer.RegisterService(typeof(IDocumentTypeResolver), new DocumentTypeResolver().WithBlipDocuments());
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
            serviceContainer.RegisterService(typeof(IHelpDeskExtension), () => new HelpDeskExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IThreadExtension), () => new ThreadExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IResourceExtension), () => new ResourceExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(ITunnelExtension), () => new TunnelExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IArtificialIntelligenceExtension), () => new ArtificialIntelligenceExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IConfigurationExtension), () => new ConfigurationExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IContextExtension), () => new ContextExtension(senderFactory()));
            serviceContainer.RegisterService(typeof(IContactsJourneyExtension), () => new ContactsJourneyExtension(senderFactory()));

            return serviceContainer;
        }
    }
}