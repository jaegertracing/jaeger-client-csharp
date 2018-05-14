using System;
using Jaeger.Core.Util;
using Xunit;

namespace Jaeger.Core.Tests
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
    }
}