using Jaeger.Encoders.Thrift;
using Jaeger.Transports.Thrift;

namespace Jaeger.Senders.Thrift
{
    public class HttpSender : ThriftSender
    {
        /// <summary>
        /// This defaults to 1 MB.
        /// </summary>
        public const int MaxPacketSize = 1048576;

        public HttpSender(string endpoint)
            : this(new ThriftHttpTransport.Builder(endpoint).Build(), 0)
        {
        }

        /// <param name="transport">If empty it will use <see cref="ThriftHttpTransport"/>.</param>
        /// <param name="maxPacketSize">If 0 it will use <see cref="MaxPacketSize"/>.</param>
        public HttpSender(ThriftHttpTransport transport, int maxPacketSize)
            : base(transport, maxPacketSize == 0 ? MaxPacketSize : maxPacketSize)
        {
        }

        public override string ToString()
        {
            return $"{nameof(HttpSender)}({base.ToString()})";
        }
    }
}
