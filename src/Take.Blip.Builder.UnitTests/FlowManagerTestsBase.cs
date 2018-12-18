using Lime.Protocol;
using NSubstitute;
using Serilog;
using SimpleInjector;
using Take.Blip.Builder.Diagnostics;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Utils;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Broadcast;
using Take.Blip.Client.Extensions.Bucket;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.EventTracker;

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class FlowManagerTestsBase : CancellationTokenTestsBase
    {
        public FlowManagerTestsBase()
        {
            BucketExtension = Substitute.For<IBucketExtension>();
            ArtificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
            EventTrackExtension = Substitute.For<IEventTrackExtension>();
            BroadcastExtension = Substitute.For<IBroadcastExtension>();
            ContactExtension = Substitute.For<IContactExtension>();
            Sender = Substitute.For<ISender>();
            StateManager = Substitute.For<IStateManager>();
            ContextProvider = Substitute.For<IContextProvider>();
            Context = Substitute.For<IContext>();
            Logger = new LoggerConfiguration().CreateLogger();
            ContextProvider
                .CreateContext(Arg.Any<Identity>(), Arg.Any<Identity>(), Arg.Any<LazyInput>(), Arg.Any<Flow>())
                .Returns(Context);
            User = new Identity("user", "domain");
            Application = new Identity("application", "domain");
            Context.User.Returns(User);
            TraceProcessor = Substitute.For<ITraceProcessor>();
        }

        public Identity User { get; set; }

        public Identity Application { get; set; }

        public IBucketExtension BucketExtension { get; set; }

        public IArtificialIntelligenceExtension ArtificialIntelligenceExtension { get; set; }

        public IEventTrackExtension EventTrackExtension { get; set; }

        public IBroadcastExtension BroadcastExtension { get; set; }

        public IContactExtension ContactExtension { get; set; }

        public ISender Sender { get; set; }

        public IStateManager StateManager { get; set; }

        public IContextProvider ContextProvider { get; set; }

        public IContext Context { get; set; }

        public ILogger Logger { get; set; }

        public virtual Container CreateContainer()
        {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;
            container.RegisterBuilder();
            container.RegisterSingleton(BucketExtension);
            container.RegisterSingleton(ArtificialIntelligenceExtension);
            container.RegisterSingleton(EventTrackExtension);
            container.RegisterSingleton(BroadcastExtension);
            container.RegisterSingleton(ContactExtension);
            container.RegisterSingleton(ContextProvider);
            container.RegisterSingleton(Sender);
            container.RegisterSingleton(StateManager);
            container.RegisterSingleton(Logger);
            return container;
        }

        public IFlowManager GetTarget()
        {
            var container = CreateContainer();
            return container.GetInstance<IFlowManager>();
        }
    }
}