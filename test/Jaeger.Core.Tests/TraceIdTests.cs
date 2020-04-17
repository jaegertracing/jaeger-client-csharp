using System;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class TraceIdTests
    {
        [Fact]
        public void InitLow_ShouldSetHighToZero()
        {
            var traceId = new TraceId(42);

            Assert.Equal(0, traceId.High);
            Assert.Equal(42, traceId.Low);
        }

        [Fact]
        public void InitHighLow_ShouldSetHighAndLow()
        {
            var traceId = new TraceId(42, 21);

            Assert.Equal(42, traceId.High);
            Assert.Equal(21, traceId.Low);
        }

        [Fact]
        public void InitLow_ToString()
        {
            var traceId = new TraceId(10);

            Assert.Equal("000000000000000a", traceId.ToString());
        }

        [Fact]
        public void InitHighLow_ToString()
        {
            var traceId = new TraceId(16, 31);

            Assert.Equal("0000000000000010000000000000001f", traceId.ToString());
        }

        [Fact]
        public void FromString_Empty()
        {
            var ex = Assert.Throws<Exception>(() => TraceId.FromString(""));
            Assert.Equal("Cannot parse Low TraceId from string: ", ex.Message);
        }

        [Fact]
        public void FromString_64Bit_WithoutZeroes()
        {
            var traceId = TraceId.FromString("1");
            Assert.Equal(0, traceId.High);
            Assert.Equal(1, traceId.Low);
        }

        [Fact]
        public void FromString_64Bit_WithZeroes()
        {
            var traceId = TraceId.FromString("0123456789abcdef");
            Assert.Equal(0, traceId.High);
            Assert.Equal(0x123456789abcdef, traceId.Low);
        }

        [Fact]
        public void FromString_64Bit_LowMalformed()
        {
            var badLowTraceId = "fedcba_876543210";
            var ex = Assert.Throws<Exception>(() => TraceId.FromString(badLowTraceId));
            Assert.Equal("Cannot parse Low TraceId from string: fedcba_876543210", ex.Message);
        }

        [Fact]
        public void FromString_MoreThan128Bit()
        {
            var tooLongTraceId = "0123456789abcdef0123456789abcdef012";
            var ex = Assert.Throws<Exception>(() => TraceId.FromString(tooLongTraceId));
            Assert.Equal($"TraceId cannot be longer than 32 hex characters: {tooLongTraceId}", ex.Message);
        }

        [Fact]
        public void FromString_128Bit_WithoutZeroes()
        {
            var traceId = TraceId.FromString("10000000000000001");
            Assert.Equal("1", traceId.High.ToString());
            Assert.Equal("1", traceId.Low.ToString());
        }

        [Fact]
        public void FromString_128Bit_WithZeroes()
        {
            var traceId = TraceId.FromString("0123456789abcdeffedcba9876543210");
            Assert.Equal("0123456789abcdef", traceId.High.ToString("x016"));
            Assert.Equal("fedcba9876543210", traceId.Low.ToString("x016"));
        }

        [Fact]
        public void FromString_128Bit_HighMalformed()
        {
            var ex = Assert.Throws<Exception>(() => TraceId.FromString("01234_6789abcdeffedcba9876543210"));
            Assert.Equal("Cannot parse High TraceId from string: 01234_6789abcdef", ex.Message);
        }

        [Fact]
        public void FromString_128Bit_LowMalformed()
        {
            var badLowTraceId = "0123456789abcdeffedcba_876543210";
            var ex = Assert.Throws<Exception>(() => TraceId.FromString(badLowTraceId));
            Assert.Equal("Cannot parse Low TraceId from string: fedcba_876543210", ex.Message);
        }
    }
}
