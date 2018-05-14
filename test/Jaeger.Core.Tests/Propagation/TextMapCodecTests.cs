using Jaeger.Core.Propagation;
using Xunit;

namespace Jaeger.Core.Tests.Propagation
{
    public class TextMapCodecTests
    {
        [Fact]
        public void TestBuilder()
        {
            TextMapCodec codec = new TextMapCodec.Builder()
                .WithUrlEncoding(true)
                .WithSpanContextKey("jaeger-trace-id")
                .WithBaggagePrefix("jaeger-baggage-")
                .Build();

            Assert.NotNull(codec);
            string str = codec.ToString();
            Assert.Contains("contextKey=jaeger-trace-id", str);
            Assert.Contains("baggagePrefix=jaeger-baggage-", str);
            Assert.Contains("urlEncoding=true", str);
        }

        [Fact]
        public void TestWithoutBuilder()
        {
            TextMapCodec codec = new TextMapCodec(false);
            string str = codec.ToString();
            Assert.Contains("contextKey=uber-trace-id", str);
            Assert.Contains("baggagePrefix=uberctx-", str);
            Assert.Contains("urlEncoding=false", str);
        }
    }
}
