using System.Collections.Generic;
using Jaeger.Core.Propagation;
using OpenTracing.Propagation;
using Xunit;

namespace Jaeger.Core.Tests.Propagation
{
    public class B3TextMapCodecResiliencyTests
    {
        private B3TextMapCodec _sut = new B3TextMapCodec();

        public static List<object[]> MaliciousInputs() => new List<object[]>
        {
            new object[] { B3TextMapCodec.TraceIdName, "abcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbd" },
            new object[] { B3TextMapCodec.TraceIdName, "" },
            new object[] { B3TextMapCodec.TraceIdName, "ABCDEF" },
            new object[] { B3TextMapCodec.SpanIdName, "abcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbd" },
            new object[] { B3TextMapCodec.SpanIdName, "" },
            new object[] { B3TextMapCodec.SpanIdName, "ABCDEF" },
            new object[] { B3TextMapCodec.ParentSpanIdName, "abcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbdabcbd" },
            new object[] { B3TextMapCodec.ParentSpanIdName, "" },
            new object[] { B3TextMapCodec.ParentSpanIdName, "ABCDEF" }
        };

        [Theory]
        [MemberData(nameof(MaliciousInputs))]
        public void ShouldFallbackWhenMaliciousInput(string headerName, string maliciousInput)
        {
            ITextMap maliciousCarrier = ValidHeaders();
            maliciousCarrier.Set(headerName, maliciousInput);
            //when
            SpanContext extract = _sut.Extract(maliciousCarrier);
            //then
            Assert.Null(extract);
        }

        private ITextMap ValidHeaders()
        {
            ITextMap maliciousCarrier = new B3TextMapCodecTest.DelegatingTextMap();
            string validInput = "ffffffffffffffffffffffffffffffff";
            maliciousCarrier.Set(B3TextMapCodec.TraceIdName, validInput);
            maliciousCarrier.Set(B3TextMapCodec.SpanIdName, validInput);
            maliciousCarrier.Set(B3TextMapCodec.ParentSpanIdName, validInput);
            return maliciousCarrier;
        }
    }
}
