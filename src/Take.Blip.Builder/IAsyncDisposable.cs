using System;
using System.Threading.Tasks;

namespace Take.Blip.Builder
{
    public interface IAsyncDisposable : IDisposable
    {
        Task DisposeAsync();
    }
}