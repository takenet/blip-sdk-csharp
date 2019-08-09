using System;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using NSubstitute;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ActionTestsBase : ContextTestsBase
    {
        public ActionTestsBase()
        {
            Context.Flow.Returns(Flow);
            From = UserIdentity.ToNode();
            To = OwnerIdentity.ToNode();
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
        
        public Node From { get; }

        public Node To { get; }

        public Message Message { get; }
        
        public LazyInput Input { get; }
    }
}
