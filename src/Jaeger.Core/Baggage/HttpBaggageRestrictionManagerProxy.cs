using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jaeger.Core.Baggage.Http;
using Jaeger.Core.Util;
using Newtonsoft.Json;

namespace Jaeger.Core.Baggage
{
    public class HttpBaggageRestrictionManagerProxy : IBaggageRestrictionManagerProxy
    {
        private const string DefaultHostPort = "localhost:5778";

        private readonly IHttpClient _httpClient;
        private readonly string _hostPort;

        public HttpBaggageRestrictionManagerProxy(IHttpClient httpClient, string hostPort)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _hostPort = hostPort ?? DefaultHostPort;
        }

        internal List<BaggageRestrictionResponse> ParseJson(string json)
        {
            return JsonConvert.DeserializeObject<List<BaggageRestrictionResponse>>(json);
        }

        public async Task<List<BaggageRestrictionResponse>> GetBaggageRestrictionsAsync(string serviceName)
        {
            string url = "http://" + _hostPort + "/baggageRestrictions?service=" + Uri.EscapeDataString(serviceName);
            string jsonString = await _httpClient.MakeGetRequestAsync(url).ConfigureAwait(false);

            return ParseJson(jsonString);
        }
    }
}