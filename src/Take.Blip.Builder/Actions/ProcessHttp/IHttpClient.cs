using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Actions.ProcessHttp
{
    public interface IHttpClient : IDisposable
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
    }
}