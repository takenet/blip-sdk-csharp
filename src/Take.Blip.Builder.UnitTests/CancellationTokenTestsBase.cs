using System;
using System.Threading;

namespace Take.Blip.Builder.UnitTests
{
    public class CancellationTokenTestsBase : IDisposable
    {
        public CancellationTokenTestsBase()
        {
            CancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        }

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }
    }
}