using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Jaeger.ApiV2;
using Jaeger.Exceptions;
using Jaeger.Reporters.Protocols;
using GrpcProcess = Jaeger.ApiV2.Process;
using GrpcSpan = Jaeger.ApiV2.Span;

namespace Jaeger.Senders
{
    /// <summary>
    /// GrpcSender provides an implementation to transport spans over HTTP using
    /// GRPC.
    /// </summary>
    public class GrpcSender : ISender
    {
        public const string DefaultCollectorGrpcHost = "localhost";
        public const int DefaultCollectorGrpcPort = 14250;

        private readonly Channel _channel;
        private readonly CollectorService.CollectorServiceClient _client;
        private GrpcProcess _process;
        private readonly List<GrpcSpan> _spanBuffer = new List<GrpcSpan>();

        /// <summary>
        /// This constructor expects Jaeger running on <see cref="DefaultCollectorGrpcHost"/>
        /// and port <see cref="DefaultCollectorGrpcPort"/> without credentials.
        /// </summary>
        public GrpcSender()
            : this(DefaultCollectorGrpcHost, DefaultCollectorGrpcPort, ChannelCredentials.Insecure)
        {
        }

        /// <param name="host">If empty it will use <see cref="DefaultCollectorGrpcHost"/>.</param>
        /// <param name="port">If 0 it will use <see cref="DefaultCollectorGrpcPort"/>.</param>
        /// <param name="credentials">If empty it will use <see cref="ChannelCredentials.Insecure"/>.</param>
        public GrpcSender(string host, int port, ChannelCredentials credentials)
        {

            if (string.IsNullOrEmpty(host))
            {
                host = DefaultCollectorGrpcHost;
            }

            if (port == 0)
            {
                port = DefaultCollectorGrpcPort;
            }

            if (credentials == null)
            {
                credentials = ChannelCredentials.Insecure;
            }

            _channel = new Channel(host, port, credentials);
            _client = new CollectorService.CollectorServiceClient(_channel);
        }

        public Task<int> AppendAsync(Span span, CancellationToken cancellationToken)
        {
            if (_process == null)
            {
                _process = new GrpcProcess
                {
                    ServiceName = span.Tracer.ServiceName,
                    Tags = { JaegerGrpcSpanConverter.BuildTags(span.Tracer.Tags) }
                };
            }

            GrpcSpan grpcSpan = JaegerGrpcSpanConverter.ConvertSpan(span);
            _spanBuffer.Add(grpcSpan);

            return Task.FromResult(0);
        }

        public async Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            if (_spanBuffer.Count == 0)
            {
                return 0;
            }

            int n = _spanBuffer.Count;
            try
            {
                var request = new PostSpansRequest
                {
                    Batch = new Batch
                    {
                        Process = _process,
                        Spans = { _spanBuffer }
                    }
                };
                await _client.PostSpansAsync(request, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                throw new SenderException("Failed to flush spans.", ex, n);
            }
            finally
            {
                _spanBuffer.Clear();
            }
            return n;
        }

        public virtual Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            return FlushAsync(cancellationToken);
        }

        public override string ToString()
        {
            return $"{nameof(GrpcSender)}()";
        }
    }
}
