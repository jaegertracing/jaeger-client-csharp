using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Encoders.Thrift;
using Jaeger.Thrift.Agent;
using Jaeger.Exceptions;
using Jaeger.Senders.SizedBatch;
using Jaeger.Senders.Thrift.Senders.Internal;
using Jaeger.Transports.Thrift;

namespace Jaeger.Senders.Thrift
{
    /// <summary>
    /// JaegerUdpTransport provides an implementation to transport spans over UDP using
    /// Compact Thrift. It handles making sure payloads efficiently use the UDP packet
    /// size by filling up as much of a UDP message it can before sending.
    /// </summary>
    public class UdpSender : ThriftSender
    {
        public const int MaxPacketSize = 65000;

        /// <summary>
        /// This constructor expects Jaeger running on <see cref="ThriftUdpTransport.DefaultAgentUdpHost"/>
        /// and port <see cref="ThriftUdpTransport.DefaultAgentUdpCompactPort"/>.
        /// </summary>
        public UdpSender()
            : this(ThriftUdpTransport.DefaultAgentUdpHost, ThriftUdpTransport.DefaultAgentUdpCompactPort, 0)
        {
        }

        /// <param name="host">If empty it will use <see cref="ThriftUdpTransport.DefaultAgentUdpHost"/>.</param>
        /// <param name="port">If 0 it will use <see cref="ThriftUdpTransport.DefaultAgentUdpCompactPort"/>.</param>
        /// <param name="maxPacketSize">If 0 it will use <see cref="MaxPacketSize"/>.</param>
        public UdpSender(string host, int port, int maxPacketSize)
            : this(new ThriftUdpTransport.Builder(host, port).Build(), maxPacketSize)
        {
        }

        /// <param name="transport">If empty it will use <see cref="ThriftUdpTransport"/>.</param>
        /// <param name="maxPacketSize">If 0 it will use <see cref="MaxPacketSize"/>.</param>
        public UdpSender(ThriftUdpTransport transport, int maxPacketSize)
            : base(transport, maxPacketSize == 0 ? MaxPacketSize : maxPacketSize)
        {
        }

        public override string ToString()
        {
            return $"{nameof(UdpSender)}({base.ToString()})";
        }
    }
}