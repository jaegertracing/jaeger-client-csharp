using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Jaeger.ApiV2;
using Jaeger.Exceptions;
using Jaeger.Senders.Grpc.Protocols;
using GrpcProcess = Jaeger.ApiV2.Process;
using GrpcSpan = Jaeger.ApiV2.Span;

namespace Jaeger.Senders.Grpc
{
    /// <summary>
    /// GrpcSender provides an implementation to transport spans over HTTP using GRPC.
    /// </summary>
    public class GrpcSender : ISender
    {
        public const string DefaultCollectorGrpcTarget = "localhost:14250";
        public const int MaxPacketSize = 65000;

        private readonly Channel _channel;
        private readonly CollectorService.CollectorServiceClient _client;
        private readonly int _maxSpanBytes;

        private readonly List<GrpcSpan> _spanBuffer = new List<GrpcSpan>();
        private GrpcProcess _process;
        private int _processBytesSize;
        private int _byteBufferSize;

        /// <summary>
        /// This constructor expects Jaeger collector running on <see cref="DefaultCollectorGrpcTarget"/> without credentials.
        /// </summary>
        public GrpcSender()
            : this(DefaultCollectorGrpcTarget, ChannelCredentials.Insecure, 0)
        {
        }

        /// <param name="target">If empty it will use <see cref="DefaultCollectorGrpcTarget"/>.</param>
        /// <param name="credentials">If empty it will use <see cref="ChannelCredentials.Insecure"/>.</param>
        /// <param name="maxPacketSize">If 0 it will use <see cref="MaxPacketSize"/>.</param>
        public GrpcSender(string target, ChannelCredentials credentials, int maxPacketSize)
        {
            if (string.IsNullOrEmpty(target))
            {
                target = DefaultCollectorGrpcTarget;
            }

            if (credentials == null)
            {
                credentials = ChannelCredentials.Insecure;
            }

            if (maxPacketSize == 0)
            {
                maxPacketSize = MaxPacketSize;
            }

            _channel = new Channel(target, credentials);
            _client = new CollectorService.CollectorServiceClient(_channel);
            _maxSpanBytes = maxPacketSize;
        }

        public async Task<int> AppendAsync(Span span, CancellationToken cancellationToken)
        {
            if (_process == null)
            {
                _process = new GrpcProcess
                {
                    ServiceName = span.Tracer.ServiceName,
                    Tags = { JaegerGrpcSpanConverter.BuildTags(span.Tracer.Tags) }
                };
                _processBytesSize = _process.CalculateSize();
                _byteBufferSize += _processBytesSize;
            }

            GrpcSpan grpcSpan = JaegerGrpcSpanConverter.ConvertSpan(span);
            int spanSize = grpcSpan.CalculateSize();
            if (spanSize > _maxSpanBytes)
            {
                throw new SenderException($"GrpcSender received a span that was too large, size = {spanSize}, max = {_maxSpanBytes}", null, 1);
            }

            _byteBufferSize += spanSize;
            if (_byteBufferSize <= _maxSpanBytes)
            {
                _spanBuffer.Add(grpcSpan);
                if (_byteBufferSize < _maxSpanBytes)
                {
                    return 0;
                }
                return await FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            int n;
            try
            {
                n = await FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (SenderException ex)
            {
                // +1 for the span not submitted in the buffer above
                throw new SenderException(ex.Message, ex, ex.DroppedSpanCount + 1);
            }

            _spanBuffer.Add(grpcSpan);
            _byteBufferSize = _processBytesSize + spanSize;
            return n;
        }

        public async Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            int n = _spanBuffer.Count;
            if (n == 0)
            {
                return 0;
            }

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
                _byteBufferSize = _processBytesSize;
            }
            return n;
        }

        public virtual Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            return FlushAsync(cancellationToken);
        }

        public override string ToString()
        {
            return $"{nameof(GrpcSender)}(ProcessBytesSize={_processBytesSize}, ByteBufferSize={_byteBufferSize})";
        }
    }
}