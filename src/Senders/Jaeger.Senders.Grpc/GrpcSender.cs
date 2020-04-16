using System.Threading;
using System.Threading.Tasks;
using Jaeger.ApiV2;
using Jaeger.Encoders.Grpc;
using Jaeger.Encoders.SizedBatch;
using Jaeger.Senders.SizedBatch;
using Jaeger.Transports;
using Jaeger.Transports.Grpc;

namespace Jaeger.Senders.Grpc
{
    /// <summary>
    /// GrpcSender provides an implementation to transport spans over HTTP using GRPC.
    /// </summary>
    public class GrpcSender : SizedBatchSender
    {
        /// <summary>
        /// Defaults to 4 MB (GRPC_DEFAULT_MAX_RECV_MESSAGE_LENGTH).
        /// </summary>
        public const int MaxPacketSize = 4 * 1024 * 1024;

        /// <summary>
        /// This constructor expects Jaeger collector running on <see cref="GrpcTransport.DefaultCollectorGrpcTarget"/> without credentials.
        /// </summary>
        public GrpcSender()
            : this(new GrpcEncoder(new GrpcTransport()), 0)
        {
        }

        /// <param name="encoder">If empty it will use <see cref="GrpcEncoder"/>.</param>
        /// <param name="maxPacketSize">If 0 it will use <see cref="MaxPacketSize"/>.</param>
        public GrpcSender(GrpcEncoder encoder, int maxPacketSize)
            : base(encoder, maxPacketSize == 0 ? MaxPacketSize : maxPacketSize)
        {
        }

        public override string ToString()
        {
            return $"{nameof(GrpcSender)}({base.ToString()})";
        }
    }
}