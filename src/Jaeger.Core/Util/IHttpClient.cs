using System.Net.Http;
using System.Threading.Tasks;

namespace Jaeger.Util
{
    public interface IHttpClient
    {
        Task<string> MakeGetRequestAsync(string urlToRead);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}