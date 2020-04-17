using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Jaeger.ApiV2;
using Jaeger.Encoders;

namespace Jaeger.Transports.Grpc
{
    public class GrpcTransport : ITransport
    {
        public const string DefaultCollectorGrpcTarget = "localhost:14250";

        private readonly Channel _channel;

        public ApiV2.CollectorService.CollectorServiceClient Client { get; }

        /// <summary>
        /// This constructor expects Jaeger collector running on <see cref="DefaultCollectorGrpcTarget"/> without credentials.
        /// </summary>
        public GrpcTransport()
            : this(DefaultCollectorGrpcTarget, ChannelCredentials.Insecure)
        {
        }

        /// <param name="target">If empty it will use <see cref="DefaultCollectorGrpcTarget"/>.</param>
        /// <param name="credentials">If empty it will use <see cref="ChannelCredentials.Insecure"/>.</param>
        public GrpcTransport(string target, ChannelCredentials credentials)
        {
            if (string.IsNullOrEmpty(target))
            {
                target = DefaultCollectorGrpcTarget;
            }

            if (credentials == null)
            {
                credentials = ChannelCredentials.Insecure;
            }

            _channel = new Channel(target, credentials);
            Client = new CollectorService.CollectorServiceClient(_channel);
        }

        public int GetSize(IEncodedData data)
        {
            var encData = (IMessage) data.Data;
            return encData.CalculateSize();
        }

        public async Task WriteBatchAsync(Batch batch, CancellationToken cancellationToken)
        {
            await Client.PostSpansAsync(new PostSpansRequest
            {
                Batch = batch
            }, cancellationToken: cancellationToken);
        }

        public override string ToString()
        {
            return $"{nameof(GrpcTransport)}(Channel={_channel.ResolvedTarget})";
        }
    }
}