using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Take.Blip.Builder.Benchmark.Context;

/// <inheritdoc />
public class CancellationTokenTestsBase : IDisposable
{
    /// <summary>
    /// The cancellation token.
    /// </summary>
    protected CancellationToken CancellationToken => CancellationTokenSource.Token;

    private CancellationTokenSource CancellationTokenSource { get; set; } =
        new(TimeSpan.FromSeconds(30));

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose the resources.
    /// </summary>
    /// <param name="disposing"></param>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    protected virtual void Dispose(bool disposing)
    {
        // Cleanup

        if (disposing)
        {
            CancellationTokenSource?.Dispose();
        }
    }
}