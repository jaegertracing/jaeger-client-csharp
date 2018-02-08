using System;
using System.Collections.Generic;
using Xunit;

namespace LetsTrace.Tests
{
    public class SpanContextTests
    {
        [Fact]
        public void SpanContext_Constructor_ShouldSetAllItemsPassedIn()
        {
            var traceId = new TraceId { High = 1, Low = 2 };
            var spanId = new SpanId(3);
            var parentId = new SpanId(4);
            var baggage = new Dictionary<string, string> { { "key", "value" } };

            var t1 = new SpanContext(traceId, spanId, parentId, baggage);
            Assert.Equal(traceId.High, t1.TraceId.High);
            Assert.Equal(traceId.Low, t1.TraceId.Low);
            Assert.Equal(spanId, t1.SpanId);
            Assert.Equal(parentId, t1.ParentId);
            Assert.Equal(baggage, t1.GetBaggageItems());
        }

        [Fact]
        public void SpanContext_Constructor_ShouldNotLetANullTraceIdBePassedIn()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SpanContext(null));
            Assert.Equal("traceId", ex.ParamName);
        }

        [Fact]
        public void SpanContext_Constructor_ShouldDefaultToEmptyBaggage()
        {
            var traceId = new TraceId { High = 1, Low = 2 };

            var t2 = new SpanContext(traceId);
            Assert.Equal(new Dictionary<string, string>(), t2.GetBaggageItems());
        }

        [Fact]
        public void SpanContext_Constructor_ShouldLetNullSpanAndParentIdsIn()
        {
            var traceId = new TraceId { High = 1, Low = 2 };

            var t3 = new SpanContext(traceId, null, null);
            Assert.Null(t3.SpanId);
            Assert.Null(t3.ParentId);
        }

        [Fact]
        public void SpanContext_ToString()
        {
            var traceId = new TraceId { High = 1, Low = 2 };
            var spanId = new SpanId(3);
            var parentId = new SpanId(4);

            var sc = new SpanContext(traceId, spanId, parentId);
            Assert.Equal("10000000000000002:3:4", sc.ToString());
        }

        [Fact]
        public void SpanContext_FromString()
        {
            var ex = Assert.Throws<Exception>(() => SpanContext.FromString(""));
            Assert.Equal("Cannot convert empty string to SpanContext", ex.Message);

            ex = Assert.Throws<Exception>(() => SpanContext.FromString("abcd"));
            Assert.Equal("String does not match tracer state format", ex.Message);

            ex = Assert.Throws<Exception>(() => SpanContext.FromString("x:1:1"));
            Assert.Equal("Cannot parse Low TraceId from string: x", ex.Message);

            ex = Assert.Throws<Exception>(() => SpanContext.FromString("1:x:1"));
            Assert.Equal("Cannot parse SpanId from string: x", ex.Message);

            ex = Assert.Throws<Exception>(() => SpanContext.FromString("1:1:x"));
            Assert.Equal("Cannot parse SpanId from string: x", ex.Message);

            var tooLongTraceId = "01234567890123456789012345678901234";
            ex = Assert.Throws<Exception>(() => SpanContext.FromString($"{tooLongTraceId}:1:1"));
            Assert.Equal($"TraceId cannot be longer than 32 hex characters: {tooLongTraceId}", ex.Message);

            var justRightTraceId = "01234567890123456789012345678901";
            var sc = SpanContext.FromString($"{justRightTraceId}:1:1");
            Assert.Equal("123456789012345", sc.TraceId.High.ToString());
            Assert.Equal("6789012345678901", sc.TraceId.Low.ToString());

            var badHighTraceId = "01234_67890123456789012345678901";
            ex = Assert.Throws<Exception>(() => SpanContext.FromString($"{badHighTraceId}:1:1"));
            Assert.Equal("Cannot parse High TraceId from string: 01234_6789012345", ex.Message);

            var badLowTraceId = "0123456789012345678901_345678901";
            ex = Assert.Throws<Exception>(() => SpanContext.FromString($"{badLowTraceId}:1:1"));
            Assert.Equal("Cannot parse Low TraceId from string: 678901_345678901", ex.Message);

            var validSpanId = "0123456789012345";
            sc = SpanContext.FromString($"1:{validSpanId}:1");
            Assert.Equal("7048860ddf79", sc.SpanId.ToString());

            var badSpanId = "01234567890123456";
            ex = Assert.Throws<Exception>(() => SpanContext.FromString($"1:{badSpanId}:1"));
            Assert.Equal($"SpanId cannot be longer than 16 hex characters: {badSpanId}", ex.Message);

            sc = SpanContext.FromString("10000000000000001:1:1");
            Assert.Equal("1", sc.TraceId.High.ToString());
            Assert.Equal("1", sc.TraceId.Low.ToString());

            sc = SpanContext.FromString("1:1:1");
            Assert.Equal("0", sc.TraceId.High.ToString());
            Assert.Equal("1", sc.TraceId.Low.ToString());
            Assert.Equal("1", sc.SpanId.ToString());
            Assert.Equal("1", sc.SpanId.ToString());
        }

        [Fact]
        public void SpanContext_SetBaggageItems()
        {
            var traceId = new TraceId { High = 1, Low = 2 };
            var spanId = new SpanId(3);
            var parentId = new SpanId(4);
            var baggage = new Dictionary<string, string> { { "key", "value" } };

            var sc = new SpanContext(traceId, spanId, parentId, baggage);

            Assert.Equal(baggage, sc.GetBaggageItems());

            var newBaggage = new Dictionary<string, string> { { "new", "baggage" } };
            sc.SetBaggageItems(newBaggage);

            Assert.Equal(newBaggage, sc.GetBaggageItems());
        }
    }
}
