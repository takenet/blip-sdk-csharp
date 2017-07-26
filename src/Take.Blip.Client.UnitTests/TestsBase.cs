using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Take.Blip.Client.UnitTests
{
    public class TestsBase : IDisposable
    {
        public TestsBase()
        {
            CancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        }

        public CancellationTokenSource CancellationTokenSource { get; }

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }
    }
}
