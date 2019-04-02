using System;
using Jaeger.Util;
using Xunit;

namespace Jaeger.Tests
{
    public class UtilsTest
    {
        [Fact]
        public void TestIpToInt32NotFourOctets()
        {
            Assert.Throws<FormatException>(() => Utils.IpToInt(":19"));
        }

        [Fact]
        public void TestIpToInt32EmptyIpException()
        {
            Assert.Throws<ArgumentNullException>(() => Utils.IpToInt(""));
        }

        [Fact]
        public void TestIpToInt32_localhost()
        {
            Assert.Equal((127 << 24) | 1, Utils.IpToInt("127.0.0.1"));
        }

        [Fact]
        public void TestIpToInt32_above127()
        {
            Assert.Equal((10 << 24) | (137 << 16) | (1 << 8) | 2, Utils.IpToInt("10.137.1.2"));
        }

        [Fact]
        public void TestIpToInt32_zeros()
        {
            Assert.Equal(0, Utils.IpToInt("0.0.0.0"));
        }

        [Fact]
        public void TestIpToInt32_broadcast()
        {
            Assert.Equal(-1, Utils.IpToInt("255.255.255.255"));
        }

        [Fact]
        public void TestLongToNetworkBytes()
        {
            Assert.Collection(Utils.LongToNetworkBytes(unchecked((long)0xC96C5795D7870F42)),
                b => Assert.Equal(b, (byte)0xC9),
                b => Assert.Equal(b, (byte)0x6C),
                b => Assert.Equal(b, (byte)0x57),
                b => Assert.Equal(b, (byte)0x95),
                b => Assert.Equal(b, (byte)0xD7),
                b => Assert.Equal(b, (byte)0x87),
                b => Assert.Equal(b, (byte)0x0F),
                b => Assert.Equal(b, (byte)0x42));
        }
    }
}