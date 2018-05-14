using System;
using System.Threading.Tasks;
using Jaeger.Core.Samplers.HTTP;
using Jaeger.Core.Util;
using Newtonsoft.Json;

namespace Jaeger.Core.Samplers
{
    public class HttpSamplingManager : ISamplingManager
    {
        public const string DefaultHostPort = "localhost:5778";

        private readonly IHttpClient _httpClient;
        private readonly string _hostPort;

        public HttpSamplingManager(string hostPort = DefaultHostPort)
            : this(new DefaultHttpClient(), hostPort)
        {
        }

        public HttpSamplingManager(IHttpClient httpClient, string hostPort = DefaultHostPort)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _hostPort = hostPort ?? DefaultHostPort;
        }

        internal SamplingStrategyResponse ParseJson(string json)
        {
            return JsonConvert.DeserializeObject<SamplingStrategyResponse>(json);
        }

        public async Task<SamplingStrategyResponse> GetSamplingStrategyAsync(string serviceName)
        {
            string url = "http://" + _hostPort + "/?service=" + Uri.EscapeDataString(serviceName);
            string jsonString = await _httpClient.MakeGetRequestAsync(url).ConfigureAwait(false);

            return ParseJson(jsonString);
        }
    }
}
