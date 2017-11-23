using System;
using System.Collections.Generic;
using System.Threading;
using Lime.Protocol;
using NSubstitute;
using SimpleInjector;
using Take.Blip.Builder.Hosting;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Take.Blip.Client.Extensions.Bucket;

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class FlowManagerTestsBase : IDisposable
    {
        public FlowManagerTestsBase()
        {
            BucketExtension = Substitute.For<IBucketExtension>();
            ArtificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
            Sender = Substitute.For<ISender>();
            StateManager = Substitute.For<IStateManager>();
            ContextProvider = Substitute.For<IContextProvider>();
            Context = Substitute.For<IContext>();
            ContextProvider
                .GetContext(Arg.Any<Identity>(), Arg.Any<string>(), Arg.Any<IDictionary<string, string>>())
                .Returns(Context);
            User = new Identity("user", "domain");
            Context.User.Returns(User);

            CancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        }

        public Identity User { get; set; }

        public IBucketExtension BucketExtension { get; set; }

        public IArtificialIntelligenceExtension ArtificialIntelligenceExtension { get; set; }

        public ISender Sender { get; set; }

        public IStateManager StateManager { get; set; }

        public IContextProvider ContextProvider { get; set; }

        public IContext Context { get; set; }

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public IFlowManager GetTarget()
        {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;
            container.RegisterBuilder();
            container.RegisterSingleton(BucketExtension);
            container.RegisterSingleton(ArtificialIntelligenceExtension);
            container.RegisterSingleton(ContextProvider);
            container.RegisterSingleton(Sender);
            container.RegisterSingleton(StateManager);
            return container.GetInstance<IFlowManager>();
        }

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }
    }
}
