using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Jaeger.Thrift.Senders.Internal;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Thrift.Transports;
using Xunit;

namespace Jaeger.Transport.Thrift.Tests.Transport.Internal
{
    public class ThriftUdpClientTransportTests : IDisposable
    {
        private MemoryStream _testingMemoryStream = new MemoryStream();
        private readonly IUdpClient _mockClient = Substitute.For<IUdpClient>();

        public void Dispose()
        {
            _testingMemoryStream?.Dispose();
        }

        [Fact]
        public void Constructor_ShouldConnectClient()
        {
            var host = "host, yo";
            var port = 4528;

            new ThriftUdpClientTransport(host, port, _testingMemoryStream, _mockClient);

            _mockClient.Received(1).Connect(Arg.Is(host), Arg.Is(port));
        }

        [Fact]
        public void Close_ShouldCloseClient()
        {
            var host = "host, yo";
            var port = 4528;

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, _mockClient);
            transport.Close();

            _mockClient.Received(1).Close();
        }

        [Fact]
        public async void ReadAsync_ShouldReadMemoryStreamIntoBufferAndReturnSize()
        {
            var host = "host, yo";
            var port = 4528;
            var mockClient = Substitute.For<IUdpClient>();
            var curBuffer = new byte[] {0x20, 0x10, 0x40, 0x30, 0x18, 0x14, 0x10};
            _testingMemoryStream = new MemoryStream(curBuffer);
            var newBuffer = new byte[7];

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, mockClient);
            var size = await transport.ReadAsync(newBuffer, 0, 7, CancellationToken.None);

            Assert.Equal(7, size);
            Assert.Equal(curBuffer, newBuffer);
        }

        [Fact]
        public async void ReadAsync_ShouldReceiveFromClientIfMemoryStreamReturnsZero()
        {
            var host = "host, yo";
            var port = 4528;
            var clientBuffer = new byte[] { 0x20, 0x10, 0x40, 0x30, 0x18, 0x14, 0x10, 0x28 };
            var clientResult = new UdpReceiveResult(clientBuffer, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));
            _mockClient.ReceiveAsync().Returns(clientResult);
            var newBuffer = new byte[8];

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, _mockClient);
            var size = await transport.ReadAsync(newBuffer, 0, 8, CancellationToken.None);

            Assert.Equal(8, size);
            Assert.Equal(clientBuffer, newBuffer);
        }

        [Fact]
        public async void ReadAsync_ShouldThrowWhenAnIOExceptionHappens()
        {
            var host = "host, yo";
            var port = 4528;
            _mockClient.ReceiveAsync().Throws(new IOException("error message, yo"));
            var newBuffer = new byte[8];

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, _mockClient);
            var ex = await Assert.ThrowsAsync<TTransportException>(() => transport.ReadAsync(newBuffer, 0, 8, CancellationToken.None));

            Assert.Equal("ERROR from underlying socket. error message, yo", ex.Message);
        }

        [Fact]
        public async void WriteAsync_ShouldWriteToMemoryStream()
        {
            var host = "host, yo";
            var port = 4528;
            var writeBuffer = new byte[] { 0x20, 0x10, 0x40, 0x30, 0x18, 0x14, 0x10, 0x28 };
            var readBuffer = new byte[8];

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, _mockClient);

            await transport.WriteAsync(writeBuffer, CancellationToken.None);
            _testingMemoryStream.Seek(0, SeekOrigin.Begin);
            var size = await _testingMemoryStream.ReadAsync(readBuffer, 0, 8, CancellationToken.None);

            Assert.Equal(8, size);
            Assert.Equal(writeBuffer, readBuffer);
        }

        [Fact]
        public async void FlushAsync_ShouldReturnWhenNothingIsInTheStream()
        {
            var host = "host, yo";
            var port = 4528;

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, _mockClient);
            var tInfo = transport.FlushAsync();

            Assert.True(tInfo.IsCompleted);
            await _mockClient.Received(0).SendAsync(Arg.Any<byte[]>(), Arg.Any<int>());
        }

        [Fact]
        public async void FlushAsync_ShouldSendStreamBytes()
        {
            var host = "host, yo";
            var port = 4528;
            var streamBytes = new byte[] { 0x20, 0x10, 0x40, 0x30, 0x18, 0x14, 0x10, 0x28 };
            _testingMemoryStream = new MemoryStream(streamBytes);

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, _mockClient);
            var tInfo = transport.FlushAsync();

            Assert.True(tInfo.IsCompleted);
            await _mockClient.Received(1).SendAsync(Arg.Any<byte[]>(), Arg.Is(8));
        }

        [Fact]
        public async void FlushAsync_ShouldThrowWhenClientDoes()
        {
            var host = "host, yo";
            var port = 4528;
            var streamBytes = new byte[] { 0x20, 0x10, 0x40, 0x30, 0x18, 0x14, 0x10, 0x28 };
            _testingMemoryStream = new MemoryStream(streamBytes);

            _mockClient.SendAsync(Arg.Any<byte[]>(), Arg.Any<int>()).Throws(new Exception("message, yo"));

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, _mockClient);
            var ex = await Assert.ThrowsAsync<TTransportException>(() => transport.FlushAsync());

            Assert.Equal("Cannot flush closed transport. message, yo", ex.Message);
        }

        [Fact]
        public void Dispose_ShouldCloseClientAndDisposeMemoryStream()
        {
            var host = "host, yo";
            var port = 4528;

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, _mockClient);
            transport.Dispose();

            _mockClient.Received(1).Dispose();
            Assert.False(_testingMemoryStream.CanRead);
            Assert.False(_testingMemoryStream.CanSeek);
            Assert.False(_testingMemoryStream.CanWrite);
        }

        [Fact]
        public void Dispose_ShouldNotTryToDisposeResourcesMoreThanOnce()
        {
            var host = "host, yo";
            var port = 4528;

            var transport = new ThriftUdpClientTransport(host, port, _testingMemoryStream, _mockClient);
            transport.Dispose();
            transport.Dispose();

            _mockClient.Received(1).Dispose();
            Assert.False(_testingMemoryStream.CanRead);
            Assert.False(_testingMemoryStream.CanSeek);
            Assert.False(_testingMemoryStream.CanWrite);
        }
    }
}
