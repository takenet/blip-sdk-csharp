using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using NSubstitute;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Benchmark.Context;

/// <inheritdoc />
public class ActionTestsBase : ContextTestsBase
{
    /// <inheritdoc />
    protected ActionTestsBase()
    {
        Context.Flow.Returns(Flow);
        From = UserIdentity.ToNode();
        To = OwnerIdentity.ToNode();
        Message = new Message()
        {
            From = From, To = To, Content = new PlainText { Text = "Hello BLiP" }
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

    private Node From { get; }

    private Node To { get; }

    private Message Message { get; }

    private LazyInput Input { get; }
}