using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Jaeger.Thrift.Senders.Internal
{
    internal class UdpClientAdapter : IUdpClient
    {
        private readonly UdpClient _client;

        public UdpClientAdapter()
        {
            _client = new UdpClient();
        }

        public void Dispose() => _client.Dispose();

        public void Connect(string host, int port) => _client.Connect(host, port);

        public Socket Client => _client.Client;

        public void Close() => _client.Close();

        public Task<UdpReceiveResult> ReceiveAsync() => _client.ReceiveAsync();

        public Task SendAsync(byte[] bytes, int bytesLength) => _client.SendAsync(bytes, bytesLength);
    }
}
