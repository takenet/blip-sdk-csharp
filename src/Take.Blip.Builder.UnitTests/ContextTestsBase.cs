using System;
using System.Collections.Generic;
using Lime.Protocol;
using NSubstitute;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.UnitTests
{
    public class ContextTestsBase : CancellationTokenTestsBase
    {
        public ContextTestsBase()
        {
            Context = Substitute.For<IContext>();
            Flow = new Flow();
            Flow.Configuration = new Dictionary<string, string>();
            UserIdentity = new Identity(Guid.NewGuid().ToString(), "msging.net");
            OwnerIdentity = new Identity("application", "msging.net");
            Context.Flow.Returns(Flow);
            Context.UserIdentity.Returns(UserIdentity);
            Context.OwnerIdentity.Returns(OwnerIdentity);
        }
        
        public IContext Context { get; }
        
        public Flow Flow { get; }
        
        public Identity UserIdentity { get; }

        public Identity OwnerIdentity { get; }
    }
}