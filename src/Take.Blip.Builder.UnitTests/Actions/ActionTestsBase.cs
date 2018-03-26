using NSubstitute;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ActionTestsBase : CancellationTokenTestsBase
    {
        public ActionTestsBase()
        {
            Context = Substitute.For<IContext>();
        }

        public IContext Context { get; }
    }
}
