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
            Context = Substitute.For<IContext>();
            From = new Node(Guid.NewGuid().ToString(), "msging.net", "");
            Message = new Message()
            {
                From = From
            };
            Input = new LazyInput(
                Message,
                new BuilderConfiguration(),
                new DocumentSerializer(new DocumentTypeResolver()),
                new EnvelopeSerializer(new DocumentTypeResolver()),
                null,
                CancellationToken);
            Context.Input.Returns(Input);
        }

        public IContext Context { get; }

        public Node From { get; }

        public Message Message { get; set; }
        
        public LazyInput Input { get; set; }
    }
}
