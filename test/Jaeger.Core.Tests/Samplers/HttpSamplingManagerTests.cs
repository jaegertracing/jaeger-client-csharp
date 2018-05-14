using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jaeger.Core.Samplers;
using Jaeger.Core.Samplers.HTTP;
using Jaeger.Core.Util;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.Core;
using Xunit;

namespace Jaeger.Core.Tests.Samplers
{
    public class HttpSamplingManagerTests
    {
        private readonly IHttpClient _httpClient;
        private readonly HttpSamplingManager _undertest;

        public HttpSamplingManagerTests()
        {
            _httpClient = Substitute.For<IHttpClient>();
            _undertest = new HttpSamplingManager(_httpClient, "www.example.com");
        }

        [Fact]
        public async Task TestGetSamplingStrategy()
        {
            _httpClient.MakeGetRequestAsync("http://www.example.com/?service=clairvoyant")
               .Returns("{\"strategyType\":0,\"probabilisticSampling\":{\"samplingRate\":0.001},\"rateLimitingSampling\":null}");

            SamplingStrategyResponse response = await _undertest.GetSamplingStrategyAsync("clairvoyant");
            Assert.NotNull(response.ProbabilisticSampling);
        }

        [Fact]
        public async Task TestGetSamplingStrategyError()
        {
            _httpClient.MakeGetRequestAsync("http://www.example.com/?service=")
                .Returns(new Func<CallInfo, string>(_ => { throw new InvalidOperationException(); }));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _undertest.GetSamplingStrategyAsync(""));
        }

        [Fact]
        public void TestParseProbabilisticSampling()
        {
            SamplingStrategyResponse response = _undertest.ParseJson(ReadFixture("probabilistic_sampling.json"));
            Assert.Equal(new ProbabilisticSamplingStrategy(0.01), response.ProbabilisticSampling);
            Assert.Null(response.RateLimitingSampling);
        }

        [Fact]
        public void TestParseRateLimitingSampling()
        {
            SamplingStrategyResponse response = _undertest.ParseJson(ReadFixture("ratelimiting_sampling.json"));
            Assert.Equal(new RateLimitingSamplingStrategy(2.1), response.RateLimitingSampling);
            Assert.Null(response.ProbabilisticSampling);
        }

        [Fact]
        public void TestParseInvalidJson()
        {
            Assert.Throws<JsonReaderException>(() => _undertest.ParseJson("invalid json"));
        }

        [Fact]
        public void TestParsePerOperationSampling()
        {
            SamplingStrategyResponse response = _undertest.ParseJson(ReadFixture("per_operation_sampling.json"));
            OperationSamplingParameters actual = response.OperationSampling;
            Assert.Equal(0.001, actual.DefaultSamplingProbability, 4);
            Assert.Equal(0.001666, actual.DefaultLowerBoundTracesPerSecond, 4);

            List<PerOperationSamplingParameters> actualPerOperationStrategies = actual.PerOperationStrategies;
            Assert.Equal(2, actualPerOperationStrategies.Count);
            Assert.Equal(
                new PerOperationSamplingParameters("GET:/search", new ProbabilisticSamplingStrategy(1.0)),
                actualPerOperationStrategies[0]);
            Assert.Equal(
                new PerOperationSamplingParameters("PUT:/pacifique", new ProbabilisticSamplingStrategy(0.8258308134813166)),
                actualPerOperationStrategies[1]);
        }

        [Fact]
        public async Task TestDefaultConstructor()
        {
            _httpClient.MakeGetRequestAsync("http://localhost:5778/?service=name")
                .Returns(new Func<CallInfo, string>(_ => { throw new InvalidOperationException(); }));

            HttpSamplingManager httpSamplingManager = new HttpSamplingManager(_httpClient);
            await Assert.ThrowsAsync<InvalidOperationException>(() => httpSamplingManager.GetSamplingStrategyAsync("name"));
        }

        private string ReadFixture(string fixtureName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName().Name;

            using (var resourceStream = assembly.GetManifestResourceStream($"{assemblyName}.Samplers.Resources.{fixtureName}"))
            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
