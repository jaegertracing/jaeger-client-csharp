using System;
using System.Threading.Tasks;
using Jaeger.Samplers.HTTP;
using Jaeger.Util;
using Newtonsoft.Json;

namespace Jaeger.Samplers
{
    public class HttpSamplingManager : ISamplingManager
    {
        public const string DefaultHostPort = "127.0.0.1:5778";
        public const string DefaultEndpoint = "http://" + DefaultHostPort + "/sampling";

        private readonly IHttpClient _httpClient;
        private readonly string _endpoint;

        public HttpSamplingManager(string endpoint = DefaultEndpoint)
            : this(new DefaultHttpClient(), endpoint)
        {
        }

        public HttpSamplingManager(IHttpClient httpClient, string endpoint = DefaultEndpoint)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _endpoint = endpoint ?? DefaultEndpoint;

            // Workaround for obsolete HostPort notation.
            // UriBuilder needs the schema if host is not an IP and port is given:
            if (!_endpoint.StartsWith("http://") && !_endpoint.StartsWith("https://"))
            {
                _endpoint = "http://" + _endpoint;
            }
        }

        internal SamplingStrategyResponse ParseJson(string json)
        {
            return JsonConvert.DeserializeObject<SamplingStrategyResponse>(json);
        }

        public async Task<SamplingStrategyResponse> GetSamplingStrategyAsync(string serviceName)
        {
            Uri uri = new UriBuilder(_endpoint) {Query = "service=" + Uri.EscapeDataString(serviceName)}.Uri;
            string jsonString = await _httpClient.MakeGetRequestAsync(uri.AbsoluteUri).ConfigureAwait(false);

            return ParseJson(jsonString);
        }

        public override string ToString()
        {
            return $"{nameof(HttpSamplingManager)}(Endpoint={_endpoint})";
        }
    }
}
