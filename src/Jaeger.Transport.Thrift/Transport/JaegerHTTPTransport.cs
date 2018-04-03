using System;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Transport.Thrift.Serialization;
using Jaeger.Transport.Thrift.Transport.Sender;
using JaegerSpan = Jaeger.Thrift.Span;

namespace Jaeger.Transport.Thrift.Transport
{
    /// <inheritdoc />
    /// <summary>
    /// JaegerHttpTransport provides an implementation to transport spans over HTTP using
    /// Thrift. Spans are buffered unitl reaching the BatchSize at which point all spans
    /// in the buffer will be sent to Jaeger.
    /// </summary>
    public class JaegerHttpTransport : JaegerThriftTransport
    {
        public Uri Uri { get; }
        internal const int DefaultBatchSize = 100;
        internal readonly int BatchSize;

        /// <inheritdoc />
        /// <param name="batchSize">The number of spans to buffer before sending them to Jaeger</param>
        /// <param name="host">The host the Jaeger is running on</param>
        /// <param name="port">The port of the host that Jaeger will accept http thrift</param>
        public JaegerHttpTransport(int batchSize = 0, string host = TransportConstants.DefaultAgentHost, int port = TransportConstants.DefaultCollectorHttpJaegerThriftPort) 
            : this(new Uri($"http://{host}:{port}/api/traces"), batchSize)
        {
        }

        /// <inheritdoc />
        /// <param name="uri">The endpoint to send http Thrift to</param>
        /// <param name="batchSize">The number of spans to buffer before sending them to Jaeger</param>
        public JaegerHttpTransport(Uri uri, int batchSize = 0) : this(uri, batchSize, new JaegerThriftHttpSender(uri))
        {
        }

        /// <inheritdoc />
        /// <param name="uri">The endpoint to send http Thrift to</param>
        /// <param name="batchSize">The number of spans to buffer before sending them to Jaeger</param>
        /// <param name="sender">The ISender that will take care of the actual sending as well as storing the items to be sent</param>
        /// <param name="serialization">The object that is used to serialize the spans into Thrift</param>
        internal JaegerHttpTransport(Uri uri, int batchSize, ISender sender, ISerialization serialization = null)
            : base(sender, serialization)
        {
            Uri = uri;
            if (batchSize <= 0)
            {
                BatchSize = DefaultBatchSize;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// ProtocolAppendLogicAsync contains the specific logic for appending spans to be
        /// sent over HTTP. It will keep track of the number of spans and send them once
        /// over the batchSize.
        /// </summary>
        /// <param name="span">The span serialized in Jaeger Thrift</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal override async Task<int> ProtocolAppendLogicAsync(JaegerSpan span, CancellationToken cancellationToken)
        {
            var curBuffCount = _sender.BufferItem(span);

            if (curBuffCount >= BatchSize) {
                return await _sender.FlushAsync(_process, cancellationToken);
            }

            return 0;
        }
    }
}
