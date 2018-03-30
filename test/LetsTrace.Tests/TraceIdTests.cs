using System;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class TraceIdTests
    {
        [Fact]
        public void TraceId_InitLow_ShouldSetHighToZero()
        {
            var traceId = new TraceId(42);

            Assert.Equal((ulong)0, traceId.High);
            Assert.Equal((ulong)42, traceId.Low);
        }

        [Fact]
        public void TraceId_InitHighLow_ShouldSetHighAndLow()
        {
            var traceId = new TraceId(42, 21);

            Assert.Equal((ulong)42, traceId.High);
            Assert.Equal((ulong)21, traceId.Low);
        }

        [Fact]
        public void TraceId_InitLow_ShouldBeInvalidOnAllZero()
        {
            var traceId = new TraceId(0);

            Assert.False(traceId.IsValid);
        }

        [Fact]
        public void TraceId_InitHighLow_ShouldBeInvalidOnAllZero()
        {
            var traceId = new TraceId(0, 0);

            Assert.False(traceId.IsValid);
        }

        [Fact]
        public void TraceId_InitLow_ShouldBeValidOnNonZeroLow()
        {
            var traceId = new TraceId(1);

            Assert.True(traceId.IsValid);
        }

        [Fact]
        public void TraceId_InitHighLow_ShouldBeValidOnNonZeroHigh()
        {
            var traceId = new TraceId(1, 0);

            Assert.True(traceId.IsValid);
        }

        [Fact]
        public void TraceId_InitHighLow_ShouldBeValidOnAllNonZero()
        {
            var traceId = new TraceId(1, 1);

            Assert.True(traceId.IsValid);
        }

        [Fact]
        public void TraceId_InitLow_ToString()
        {
            var traceId = new TraceId(10);

            Assert.Equal("a", traceId.ToString());
        }

        [Fact]
        public void TraceId_InitHighLow_ToString()
        {
            var traceId = new TraceId(16, 31);

            Assert.Equal("10000000000000001f", traceId.ToString());
        }

        [Fact]
        public void TraceId_FromString()
        {
            var ex = Assert.Throws<Exception>(() => TraceId.FromString(""));
            Assert.Equal("Cannot parse Low TraceId from string: ", ex.Message);

            var tooLongTraceId = "0123456789abcdef0123456789abcdef012";
            ex = Assert.Throws<Exception>(() => TraceId.FromString(tooLongTraceId));
            Assert.Equal($"TraceId cannot be longer than 32 hex characters: {tooLongTraceId}", ex.Message);

            var traceId = TraceId.FromString("0123456789abcdeffedcba9876543210");
            Assert.Equal("0123456789abcdef", traceId.High.ToString("x016"));
            Assert.Equal("fedcba9876543210", traceId.Low.ToString("x016"));

            ex = Assert.Throws<Exception>(() => TraceId.FromString("01234_6789abcdeffedcba9876543210"));
            Assert.Equal("Cannot parse High TraceId from string: 01234_6789abcdef", ex.Message);

            var badLowTraceId = "0123456789abcdeffedcba_876543210";
            ex = Assert.Throws<Exception>(() => TraceId.FromString(badLowTraceId));
            Assert.Equal("Cannot parse Low TraceId from string: fedcba_876543210", ex.Message);
            
            traceId = TraceId.FromString("10000000000000001");
            Assert.Equal("1", traceId.High.ToString());
            Assert.Equal("1", traceId.Low.ToString());
        }
    }
}
