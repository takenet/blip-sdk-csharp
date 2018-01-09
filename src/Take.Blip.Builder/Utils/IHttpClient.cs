using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Utils
{
    public interface IHttpClient : IDisposable
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}