using System;
using System.Collections.Generic;
using Lime.Protocol;
using NSubstitute;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Benchmark.Context;

/// <inheritdoc />
public class ContextTestsBase : CancellationTokenTestsBase
{
    /// <inheritdoc />
    protected ContextTestsBase()
    {
        Context = Substitute.For<IContext>();
        Flow = new Flow { Configuration = new Dictionary<string, string>() };
        UserIdentity = new Identity(Guid.NewGuid().ToString(), "msging.net");
        OwnerIdentity = new Identity("application", "msging.net");
        Context.Flow.Returns(Flow);
        Context.UserIdentity.Returns(UserIdentity);
        Context.OwnerIdentity.Returns(OwnerIdentity);
    }

    /// <summary>
    /// The context instance.
    /// </summary>
    protected IContext Context { get; set;  }

    /// <summary>
    /// The flow instance.
    /// </summary>
    protected Flow Flow { get; }

    /// <summary>
    /// The user identity.
    /// </summary>
    protected Identity UserIdentity { get; }

    /// <summary>
    /// The owner identity.
    /// </summary>
    protected Identity OwnerIdentity { get; }
}