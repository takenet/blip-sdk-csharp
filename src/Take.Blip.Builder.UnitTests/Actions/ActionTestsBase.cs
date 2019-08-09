using System;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using NSubstitute;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ActionTestsBase : CancellationTokenTestsBase
    {
        public ActionTestsBase()
        {
            Flow = new Flow();
            Context = Substitute.For<IContext>();
            Context.Flow.Returns(Flow);
            From = new Node(Guid.NewGuid().ToString(), "msging.net", "");
            To = new Node("application", "msging.net", "");
            UserIdentity = From.ToIdentity();
            OwnerIdentity = To.ToIdentity();
            Message = new Message()
            {
                From = From,
                To = To
            };
            Input = new LazyInput(
                Message,
                UserIdentity,
                new BuilderConfiguration(),
                new DocumentSerializer(new DocumentTypeResolver()),
                new EnvelopeSerializer(new DocumentTypeResolver()),
                null,
                CancellationToken);
            Context.Input.Returns(Input);
            Context.OwnerIdentity.Returns(OwnerIdentity);
            Context.UserIdentity.Returns(UserIdentity);
        }

        public IContext Context { get; }

        public Flow Flow { get; }

        public Node From { get; }

        public Node To { get; }
        
        public Identity UserIdentity { get; }

        public Identity OwnerIdentity { get; }
        
        public Message Message { get; }
        
        public LazyInput Input { get; }
    }
}
