using System.Net.Http;

namespace Take.Blip.Builder.Actions.ProcessHttp
{
    public class HttpClientWrapper : HttpClient, IHttpClient
    {
        public HttpClientWrapper()
            : base(
                new HttpClientHandler
                {
                    UseCookies = false,
                    ClientCertificateOptions = ClientCertificateOption.Manual
                })
        {
            
        }
    }
}