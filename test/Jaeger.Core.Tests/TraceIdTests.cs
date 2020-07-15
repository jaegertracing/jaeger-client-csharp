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

        [Fact]
        public void TraceId_InitLow_ShouldReturnBytes()
        {
            var traceId = TraceId.FromString("fedcba9876543210");
            Assert.Collection(traceId.ToByteArray(),
                b => Assert.Equal(b, (byte)0x00),
                b => Assert.Equal(b, (byte)0x00),
                b => Assert.Equal(b, (byte)0x00),
                b => Assert.Equal(b, (byte)0x00),
                b => Assert.Equal(b, (byte)0x00),
                b => Assert.Equal(b, (byte)0x00),
                b => Assert.Equal(b, (byte)0x00),
                b => Assert.Equal(b, (byte)0x00),
                b => Assert.Equal(b, (byte)0xfe),
                b => Assert.Equal(b, (byte)0xdc),
                b => Assert.Equal(b, (byte)0xba),
                b => Assert.Equal(b, (byte)0x98),
                b => Assert.Equal(b, (byte)0x76),
                b => Assert.Equal(b, (byte)0x54),
                b => Assert.Equal(b, (byte)0x32),
                b => Assert.Equal(b, (byte)0x10));
        }

        [Fact]
        public void TraceId_InitHighLow_ShouldReturnBytes()
        {
            var traceId = TraceId.FromString("0123456789abcdeffedcba9876543210");
            Assert.Collection(traceId.ToByteArray(),
                b => Assert.Equal(b, (byte)0x01),
                b => Assert.Equal(b, (byte)0x23),
                b => Assert.Equal(b, (byte)0x45),
                b => Assert.Equal(b, (byte)0x67),
                b => Assert.Equal(b, (byte)0x89),
                b => Assert.Equal(b, (byte)0xab),
                b => Assert.Equal(b, (byte)0xcd),
                b => Assert.Equal(b, (byte)0xef),
                b => Assert.Equal(b, (byte)0xfe),
                b => Assert.Equal(b, (byte)0xdc),
                b => Assert.Equal(b, (byte)0xba),
                b => Assert.Equal(b, (byte)0x98),
                b => Assert.Equal(b, (byte)0x76),
                b => Assert.Equal(b, (byte)0x54),
                b => Assert.Equal(b, (byte)0x32),
                b => Assert.Equal(b, (byte)0x10));
        }
    }
}
