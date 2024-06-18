using System.Net.Http;
using System.Security.Authentication;

namespace Take.Blip.Builder.Utils
{
    public class HttpClientWrapper : HttpClient, IHttpClient
    {
        public HttpClientWrapper()
            : base(
                new HttpClientHandler
                {
                    UseCookies = false,
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    SslProtocols = SslProtocols.None
                })
        {
            
        }
    }
}