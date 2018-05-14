using System;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class SpanContextTests
    {
        [Fact]
        public void TestContextFromStringMalformedException()
        {
            Assert.Throws<ArgumentException>(() => SpanContext.ContextFromString("ff:ff:ff"));
        }

        [Fact]
        public void TestContextFromStringEmptyException()
        {
            Assert.Throws<ArgumentException>(() => SpanContext.ContextFromString(""));
        }

        [Fact]
        public void TestContextFromString()
        {
            SpanContext context = SpanContext.ContextFromString("ff:dd:cc:4");
            Assert.Equal(0L, context.TraceId.High);
            Assert.Equal(255L, context.TraceId.Low);
            Assert.Equal(221L, context.SpanId);
            Assert.Equal(204L, context.ParentId);
            Assert.Equal(4, (byte)context.Flags);
        }

        [Fact]
        public void TestToStringFormatsPositiveFields()
        {
            TraceId traceId = new TraceId(-10L);
            SpanId spanId = new SpanId(-10L);
            SpanId parentId = new SpanId(-10L);
            byte flags = (byte)129;

            // I use MIN_VALUE because the most significant bit, and thats when
            // we want to make sure the hex number is positive.
            SpanContext context = new SpanContext(traceId, spanId, parentId, (SpanContextFlags)flags);

            context.ContextAsString().Split(':');

            Assert.Equal("fffffffffffffff6:fffffffffffffff6:fffffffffffffff6:81", context.ContextAsString());
            SpanContext contextFromStr = SpanContext.ContextFromString(context.ContextAsString());
            Assert.Equal(traceId, contextFromStr.TraceId);
            Assert.Equal(spanId, contextFromStr.SpanId);
            Assert.Equal(parentId, contextFromStr.ParentId);
            Assert.Equal(flags, (byte)contextFromStr.Flags);
        }
    }
}
