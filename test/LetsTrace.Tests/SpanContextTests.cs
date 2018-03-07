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
            ContextFlags flags = (ContextFlags)3;

            var t1 = new SpanContext(traceId, spanId, parentId, baggage, flags);
            Assert.Equal(traceId.High, t1.TraceId.High);
            Assert.Equal(traceId.Low, t1.TraceId.Low);
            Assert.Equal(spanId, t1.SpanId);
            Assert.Equal(parentId, t1.ParentId);
            Assert.Equal(baggage, t1.GetBaggageItems());
            Assert.Equal(flags, t1.Flags);
        }

        [Fact]
        public void SpanContext_Constructor_ShouldNotLetANullTraceIdBePassedIn()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SpanContext(null));
            Assert.Equal("traceId", ex.ParamName);
        }

        [Fact]
        public void SpanContext_Constructor_ShouldDefaultToEmptyBaggage_AndSampledFlag()
        {
            var traceId = new TraceId { High = 1, Low = 2 };

            var t2 = new SpanContext(traceId);
            Assert.Equal(new Dictionary<string, string>(), t2.GetBaggageItems());
            Assert.Equal(ContextFlags.Sampled, t2.Flags);
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
            ContextFlags flags = (ContextFlags)3;

            var sc = new SpanContext(traceId, spanId, parentId, null, flags);
            Assert.Equal("10000000000000002:3:4:3", sc.ToString());
        }

        [Fact]
        public void SpanContext_FromString()
        {
            var ex = Assert.Throws<Exception>(() => SpanContext.FromString(""));
            Assert.Equal("Cannot convert empty string to SpanContext", ex.Message);

            ex = Assert.Throws<Exception>(() => SpanContext.FromString("abcd"));
            Assert.Equal("String does not match tracer state format", ex.Message);

            ex = Assert.Throws<Exception>(() => SpanContext.FromString("x:1:1:1"));
            Assert.Equal("Cannot parse Low TraceId from string: x", ex.Message);

            ex = Assert.Throws<Exception>(() => SpanContext.FromString("1:x:1:1"));
            Assert.Equal("Cannot parse SpanId from string: x", ex.Message);

            ex = Assert.Throws<Exception>(() => SpanContext.FromString("1:1:x:1"));
            Assert.Equal("Cannot parse SpanId from string: x", ex.Message);

            var tooLongTraceId = "0123456789abcdef0123456789abcdef012";
            ex = Assert.Throws<Exception>(() => SpanContext.FromString($"{tooLongTraceId}:1:1:2"));
            Assert.Equal($"TraceId cannot be longer than 32 hex characters: {tooLongTraceId}", ex.Message);

            var justRightTraceId = "0123456789abcdeffedcba9876543210";
            var sc = SpanContext.FromString($"{justRightTraceId}:1:1:2");
            Assert.Equal("0123456789abcdef", sc.TraceId.High.ToString("x016"));
            Assert.Equal("fedcba9876543210", sc.TraceId.Low.ToString("x016"));

            var badHighTraceId = "01234_6789abcdeffedcba9876543210";
            ex = Assert.Throws<Exception>(() => SpanContext.FromString($"{badHighTraceId}:1:1:2"));
            Assert.Equal("Cannot parse High TraceId from string: 01234_6789abcdef", ex.Message);

            var badLowTraceId = "0123456789abcdeffedcba_876543210";
            ex = Assert.Throws<Exception>(() => SpanContext.FromString($"{badLowTraceId}:1:1:2"));
            Assert.Equal("Cannot parse Low TraceId from string: fedcba_876543210", ex.Message);

            var validSpanId = "0123456789abcdef";
            sc = SpanContext.FromString($"1:{validSpanId}:1:2");
            Assert.Equal("123456789abcdef", sc.SpanId.ToString());

            var badSpanId = "0123456789abcdef0";
            ex = Assert.Throws<Exception>(() => SpanContext.FromString($"1:{badSpanId}:1:2"));
            Assert.Equal($"SpanId cannot be longer than 16 hex characters: {badSpanId}", ex.Message);

            sc = SpanContext.FromString("10000000000000001:1:1:2");
            Assert.Equal("1", sc.TraceId.High.ToString());
            Assert.Equal("1", sc.TraceId.Low.ToString());
            Assert.Equal((ContextFlags)2, sc.Flags);

            sc = SpanContext.FromString("1:1:1:1");
            Assert.Equal("0", sc.TraceId.High.ToString());
            Assert.Equal("1", sc.TraceId.Low.ToString());
            Assert.Equal("1", sc.SpanId.ToString());
            Assert.Equal("1", sc.SpanId.ToString());
            Assert.Equal(ContextFlags.Sampled, sc.Flags);
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
