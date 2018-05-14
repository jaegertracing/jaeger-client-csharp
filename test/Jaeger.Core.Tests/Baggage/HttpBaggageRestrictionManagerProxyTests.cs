using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jaeger.Core.Baggage;
using Jaeger.Core.Baggage.Http;
using Jaeger.Core.Util;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.Core;
using Xunit;

namespace Jaeger.Core.Tests.Baggage
{
    public class HttpBaggageRestrictionManagerProxyTest
    {
        private readonly IHttpClient _httpClient;
        private readonly HttpBaggageRestrictionManagerProxy _undertest;
        private BaggageRestrictionResponse _expectedRestriction = new BaggageRestrictionResponse("key", 10);

        public HttpBaggageRestrictionManagerProxyTest()
        {
            _httpClient = Substitute.For<IHttpClient>();
            _undertest = new HttpBaggageRestrictionManagerProxy(_httpClient, "www.example.com");
        }

        [Fact]
        public async Task TestGetBaggageRestrictions()
        {
            _httpClient.MakeGetRequestAsync("http://www.example.com/baggageRestrictions?service=clairvoyant")
                .Returns("[{\"baggageKey\":\"key\",\"maxValueLength\":\"10\"}]");

            List<BaggageRestrictionResponse> response = await _undertest.GetBaggageRestrictionsAsync("clairvoyant");
            Assert.NotNull(response);
            Assert.Single(response);
            Assert.Equal(_expectedRestriction, response[0]);
        }

        [Fact]
        public async Task TestGetSamplingStrategyError()
        {
            _httpClient.MakeGetRequestAsync("http://www.example.com/baggageRestrictions?service=")
                .Returns(new Func<CallInfo, string>(_ => { throw new InvalidOperationException(); }));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _undertest.GetBaggageRestrictionsAsync(""));
        }

        [Fact]
        public void TestParseInvalidJson()
        {
            Assert.Throws<JsonReaderException>(() => _undertest.ParseJson("invalid json"));
        }
    }
}
