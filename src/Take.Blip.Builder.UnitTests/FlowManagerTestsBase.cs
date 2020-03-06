using System;
using System.Threading;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using NSubstitute;
using Serilog;
using SimpleInjector;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Storage;
using Take.Blip.Builder.Storage.Memory;
using Take.Blip.Client;
using Take.Blip.Client.Activation;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Broadcast;
using Take.Blip.Client.Extensions.Bucket;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.EventTracker;
using Take.Blip.Client.Extensions.HelpDesk;
using Take.Blip.Client.Extensions.Scheduler;
using Take.Blip.Client.Extensions.Tunnel;

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class FlowManagerTestsBase : CancellationTokenTestsBase
    {
        public FlowManagerTestsBase()
        {
            SchedulerExtension = Substitute.For<ISchedulerExtension>();
            BucketExtension = Substitute.For<IBucketExtension>();
            ArtificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
            EventTrackExtension = Substitute.For<IEventTrackExtension>();
            BroadcastExtension = Substitute.For<IBroadcastExtension>();
            ContactExtension = Substitute.For<IContactExtension>();
            HelpDeskExtension = Substitute.For<IHelpDeskExtension>();
            TunnelExtension = Substitute.For<ITunnelExtension>();
            Sender = Substitute.For<ISender>();
            StateManager = Substitute.For<IStateManager>();
            ContextProvider = Substitute.For<IContextProvider>();
            Context = Substitute.For<IContext>();
            Logger = new LoggerConfiguration().CreateLogger();
            ContextProvider
                .CreateContext(Arg.Any<Identity>(), Arg.Any<Identity>(), Arg.Any<LazyInput>(), Arg.Any<Flow>())
                .Returns(Context);
            UserIdentity = new Identity("user", "domain");
            ApplicationIdentity = new Identity("application", "domain");
            Application = new Application()
            {
                Identifier = ApplicationIdentity.Name,
                Domain = ApplicationIdentity.Domain
            };
            Message = new Message()
            {
                From = UserIdentity.ToNode(),
                To = ApplicationIdentity.ToNode()
            };
            Context.UserIdentity.Returns(UserIdentity);
            Input = new LazyInput(
                Message,
                UserIdentity,
                new BuilderConfiguration(),
                Substitute.For<IDocumentSerializer>(),
                Substitute.For<IEnvelopeSerializer>(),
                ArtificialIntelligenceExtension,
                CancellationToken); 
            Context.Input.Returns(Input);

            TraceProcessor = Substitute.For<ITraceProcessor>();
            OwnerCallerContactMap = new OwnerCallerContactMap();
            UserOwnerResolver = Substitute.For<IUserOwnerResolver>();
            UserOwnerResolver
                .GetUserOwnerIdentitiesAsync(Arg.Any<Message>(), Arg.Any<BuilderConfiguration>(), Arg.Any<CancellationToken>())
                .Returns(new UserOwner(UserIdentity, ApplicationIdentity));
        }

        public Identity UserIdentity { get; set; }

        public Application Application { get; set; }

        public Identity ApplicationIdentity { get; set; }

        public Message Message { get; set; }

        public LazyInput Input { get; set; }

        public ISchedulerExtension SchedulerExtension { get; set; }

        public IBucketExtension BucketExtension { get; set; }

        public IArtificialIntelligenceExtension ArtificialIntelligenceExtension { get; set; }

        public IEventTrackExtension EventTrackExtension { get; set; }

        public IBroadcastExtension BroadcastExtension { get; set; }

        public IContactExtension ContactExtension { get; set; }
        
        public IHelpDeskExtension HelpDeskExtension { get; set; }        

        public ITunnelExtension TunnelExtension { get; set; }

        public IOwnerCallerContactMap OwnerCallerContactMap { get; set; }

        public ISender Sender { get; set; }

        public IStateManager StateManager { get; set; }

        public IContextProvider ContextProvider { get; set; }

        public IContext Context { get; set; }

        public ILogger Logger { get; set; }

        public ITraceProcessor TraceProcessor { get; set; }

        public IUserOwnerResolver UserOwnerResolver { get; }

        public virtual Container CreateContainer()
        {
            var container = new Container();

            container.Options.AllowOverridingRegistrations = true;
            container.RegisterBuilder();
            container.RegisterSingleton(Application);
            container.RegisterSingleton(SchedulerExtension);
            container.RegisterSingleton(BucketExtension);
            container.RegisterSingleton(ArtificialIntelligenceExtension);
            container.RegisterSingleton(EventTrackExtension);
            container.RegisterSingleton(BroadcastExtension);
            container.RegisterSingleton(ContactExtension);
            container.RegisterSingleton(HelpDeskExtension);
            container.RegisterSingleton(TunnelExtension);
            container.RegisterSingleton(OwnerCallerContactMap);
            container.RegisterSingleton(ContextProvider);
            container.RegisterSingleton(Sender);
            container.RegisterSingleton(StateManager);
            container.RegisterSingleton(Logger);
            container.RegisterSingleton(TraceProcessor);
            container.RegisterSingleton(UserOwnerResolver);

            return container;
        }

        public IFlowManager GetTarget()
        {
            var container = CreateContainer();
            return container.GetInstance<IFlowManager>();
        }

        public IFlowManager GetTarget(Action<Container> additionalSetup)
        {
            var container = CreateContainer();
            additionalSetup(container);
            return container.GetInstance<IFlowManager>();
        }
    }
}