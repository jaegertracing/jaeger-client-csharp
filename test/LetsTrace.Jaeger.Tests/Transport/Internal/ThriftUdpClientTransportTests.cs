using System;
using System.IO;
using LetsTrace.Jaeger.Transport.Internal;
using NSubstitute;
using Xunit;

namespace LetsTrace.Jaeger.Tests.Transport.Internal
{
    public class ThriftUdpClientTransportTests : IDisposable
    {
        private readonly MemoryStream _testingMemoryStream = new MemoryStream();

        public void Dispose()
        {
            _testingMemoryStream?.Dispose();
        }

        [Fact]
        public void Constructor_ShouldConnectClient()
        {
            var host = "host, yo";
            var port = 4528;
            var mockClient = Substitute.For<IUdpClient>();

            new ThriftUdpClientTransport(host, port, _testingMemoryStream, mockClient);

            mockClient.Received(1).Connect(Arg.Is(host), Arg.Is(port));
        }

        [Fact]
        public void Close_ShouldCloseClient()
        {
            var host = "host, yo";
            var port = 4528;
            var mockClient = Substitute.For<IUdpClient>();

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, mockClient);
            transport.Close();

            mockClient.Received(1).Close();
        }
    }
}
