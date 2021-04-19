using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using Jaeger.Reporters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing.Noop;
using OpenTracing.Util;
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
