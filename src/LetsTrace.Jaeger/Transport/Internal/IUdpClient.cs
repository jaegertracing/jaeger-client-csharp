using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LetsTrace.Jaeger.Transport.Internal
{
    internal interface IUdpClient : IDisposable
    {
        void Connect(string host, int port);
        Socket Client { get; }
        void Close();
        Task<UdpReceiveResult> ReceiveAsync();
        Task SendAsync(byte[] bytes, int bytesLength);
    }
}
