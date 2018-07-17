using System.Collections;
using System.Collections.Generic;
using Jaeger.Propagation;
using OpenTracing.Propagation;
using Xunit;

namespace Jaeger.Tests.Propagation
{
    /**
     * NOTE:
     * These tests are based on the ones from jaeger-b3, and included to improve the test
     * coverage. The main testing of the B3TextMapCodec is still performed via the tests
     * in the jaeger-b3 module.
     *
     */
    public class B3TextMapCodecTest
    {
        B3TextMapCodec b3Codec = new B3TextMapCodec();

        [Fact]
        public void Downgrades128BitTraceIdToLower64Bits()
        {
            string hex128Bits = "463ac35c9f6413ad48485a3953bb6124";
            string lower64Bits = "48485a3953bb6124";

            var textMap = new TestTextMap();
            textMap.Set(B3TextMapCodec.TraceIdName, hex128Bits);
            textMap.Set(B3TextMapCodec.SpanIdName, lower64Bits);
            textMap.Set(B3TextMapCodec.ParentSpanIdName, "0");
            textMap.Set(B3TextMapCodec.SampledName, "1");
            textMap.Set(B3TextMapCodec.FlagsName, "1");

            SpanContext context = b3Codec.Extract(textMap);

            //Assert.NotNull(HexCodec.LowerHexToUnsignedLong(lower64Bits));
            Assert.Equal(HexCodec.LowerHexToUnsignedLong(lower64Bits), context.TraceId.Low);
            Assert.Equal(HexCodec.LowerHexToUnsignedLong(lower64Bits), context.SpanId);
            Assert.Equal(new SpanId(0), context.ParentId);
            Assert.True(context.Flags.HasFlag(SpanContextFlags.Sampled));
            Assert.True(context.Flags.HasFlag(SpanContextFlags.Debug));
        }

        [Fact]
        public void TestInject()
        {
            var textMap = new TestTextMap();
            b3Codec.Inject(new SpanContext(new TraceId(1), new SpanId(1), new SpanId(1), SpanContextFlags.Sampled), textMap);

            Assert.True(textMap.ContainsKey(B3TextMapCodec.TraceIdName));
            Assert.True(textMap.ContainsKey(B3TextMapCodec.SpanIdName));
        }
    }
}
