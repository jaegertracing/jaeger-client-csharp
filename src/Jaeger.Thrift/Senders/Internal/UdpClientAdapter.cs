using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Jaeger.Thrift.Senders.Internal
{
    internal class UdpClientAdapter : IUdpClient
    {
        private readonly UdpClient _client;

#if NETSTANDARD1_6
        private string _host;
        private int _port;
#endif

        public UdpClientAdapter()
        {
            _client = new UdpClient();
        }

        public void Dispose() => _client.Dispose();

        public Socket Client => _client.Client;

        public Task<UdpReceiveResult> ReceiveAsync() => _client.ReceiveAsync();

#if NETSTANDARD1_6
        public void Connect(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public Task SendAsync(byte[] bytes, int bytesLength) => _client.SendAsync(bytes, bytesLength, _host, _port);

        public void Close()
        {
            _host = null;
            _port = 0;
        }
#else
        public void Connect(string host, int port) => _client.Connect(host, port);

        public Task SendAsync(byte[] bytes, int bytesLength) => _client.SendAsync(bytes, bytesLength);

        public void Close() => _client.Close();
#endif
    }
}
