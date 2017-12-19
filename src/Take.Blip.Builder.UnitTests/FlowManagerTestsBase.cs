using System.Collections.Generic;
using Lime.Protocol;
using NSubstitute;
using SimpleInjector;
using Take.Blip.Builder.Hosting;
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
            ContextProvider
                .GetContext(Arg.Any<Identity>(), Arg.Any<string>())
                .Returns(Context);
            User = new Identity("user", "domain");
            Context.User.Returns(User);
        }

        public Identity User { get; set; }

        public IBucketExtension BucketExtension { get; set; }

        public IArtificialIntelligenceExtension ArtificialIntelligenceExtension { get; set; }

        public IEventTrackExtension EventTrackExtension { get; set; }

        public IBroadcastExtension BroadcastExtension { get; set; }

        public IContactExtension ContactExtension { get; set; }

        public ISender Sender { get; set; }

        public IStateManager StateManager { get; set; }

        public IContextProvider ContextProvider { get; set; }

        public IContext Context { get; set; }

        public IFlowManager GetTarget()
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
            return container.GetInstance<IFlowManager>();
        }
    }
}
