using System.Net.Http;

namespace Take.Blip.Builder.Utils
{
    public class HttpClientWrapper : HttpClient, IHttpClient
    {
        public HttpClientWrapper()
            : base(
#pragma warning disable CA2000 // Dispose objects before losing scope -> HttpMessageHandler is disposed inside HttpClient
                new HttpClientHandler
                {
                    UseCookies = false,
                    ClientCertificateOptions = ClientCertificateOption.Manual
                })
#pragma warning restore CA2000 // Dispose objects before losing scope
        {
            
        }
    }
}