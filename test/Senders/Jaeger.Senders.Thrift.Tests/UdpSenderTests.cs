using System;
using Xunit;

namespace Jaeger.Senders.Thrift.Tests
{
    public class UdpSenderTests
    {
        [Fact]
        public void TestSenderWithAgentDataFromEnv()
        {
            Assert.Throws<NotSupportedException>(() => new UdpSender("jaeger-agent", 6832, 65535));
        }
    }
}
