using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Transports;

namespace Jaeger.Transport.Thrift.Transport.Internal
{
    internal class ThriftUdpClientTransport : TClientTransport
    {
        private readonly IUdpClient _client;
        private readonly MemoryStream _byteStream;
        private bool _isDisposed;

        public ThriftUdpClientTransport(string host, int port) : this(host, port, new MemoryStream(), new UdpClientAdapter())
        {}

        internal ThriftUdpClientTransport(string host, int port, MemoryStream byteStream, IUdpClient udpClient)
        {
            _byteStream = byteStream;
            _client = udpClient;
            _client.Connect(host, port);
        }

        public override bool IsOpen => _client.Client.Connected;

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await Task.FromCanceled(cancellationToken);
            }
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

        public override async Task WriteAsync(byte[] buffer, CancellationToken cancellationToken) => await WriteAsync(buffer, 0, buffer.Length, cancellationToken);

        public override async Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            await _byteStream.WriteAsync(buffer, offset, length, cancellationToken);
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
