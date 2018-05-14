using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Jaeger.Core.Util
{
    public class DefaultHttpClient : IHttpClient
    {
        private const int TimeoutMs = 5000;

        private readonly HttpClient _httpClient;

        public DefaultHttpClient(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromMilliseconds(TimeoutMs) };
        }

        public async Task<string> MakeGetRequestAsync(string requestUri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Note: This ensures that internal requests from the tracer are not instrumented
            // by https://github.com/opentracing-contrib/csharp-netcore
            request.Properties["ot-ignore"] = true;

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return _httpClient.SendAsync(request);
        }
    }
}