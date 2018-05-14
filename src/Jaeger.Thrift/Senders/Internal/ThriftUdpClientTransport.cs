using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Transports;

namespace Jaeger.Thrift.Senders.Internal
{
    public class ThriftUdpClientTransport : TClientTransport
    {
        public const int MaxPacketSize = 65000; // TODO !!! Not yet used.

        private readonly IUdpClient _client;
        private readonly MemoryStream _byteStream;
        private bool _isDisposed;

        public ThriftUdpClientTransport(string host, int port)
            : this(host, port, new MemoryStream(), new UdpClientAdapter())
        {
        }

        internal ThriftUdpClientTransport(string host, int port, MemoryStream byteStream, IUdpClient udpClient)
        {
            _byteStream = byteStream;
            _client = udpClient;
            _client.Connect(host, port);
        }

        public override bool IsOpen => _client.Client.Connected;

        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override void Close()
        {
            _client.Close();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            var curDataSize = await _byteStream.ReadAsync(buffer, offset, length, cancellationToken);
            if (curDataSize != 0)
                return curDataSize;

            UdpReceiveResult result;
            try
            {
                result = await _client.ReceiveAsync();
            }
            catch (IOException e)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, $"ERROR from underlying socket. {e.Message}");
            }

            _byteStream.SetLength(0);
            await _byteStream.WriteAsync(result.Buffer, 0, result.Buffer.Length, cancellationToken);
            _byteStream.Seek(0, SeekOrigin.Begin);

            return await _byteStream.ReadAsync(buffer, offset, length, cancellationToken);

        }

        public override Task WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            return WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            return _byteStream.WriteAsync(buffer, offset, length, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            var bytes = _byteStream.ToArray();

            if (bytes.Length == 0)
                return Task.CompletedTask;

            _byteStream.SetLength(0);

            try
            {
                return _client.SendAsync(bytes, bytes.Length);
            }
            catch (SocketException se)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, $"Cannot flush because of socket exception. UDP Packet size was {bytes.Length}. Exception message: {se.Message}");
            }
            catch (Exception e)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, $"Cannot flush closed transport. {e.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                _byteStream?.Dispose();
                _client?.Dispose();
            }
            _isDisposed = true;
        }
    }
}
