using System;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Core;
using Jaeger.Core.Exceptions;
using Jaeger.Core.Transport;
using Jaeger.Transport.Thrift.Serialization;
using Jaeger.Transport.Thrift.Transport.Sender;
using JaegerProcess = Jaeger.Thrift.Process;
using JaegerSpan = Jaeger.Thrift.Span;

namespace Jaeger.Transport.Thrift.Transport
{
    /// <summary>
    /// JaegerThriftTransport is the base class for transporting spans from C# into Jaeger.
    /// It provides the basic implementation of serializing spans but leaves the buffering
    /// and sending of the serialized spans to concrete implementations.
    /// </summary>
    public abstract class JaegerThriftTransport : ITransport
    {
        private readonly ISerialization _jaegerThriftSerialization;
        protected readonly ISender _sender;
        protected JaegerProcess _process;

        /// <param name="sender">The ISender that will take care of the actual sending as well as storing the items to be sent</param>
        /// <param name="serialization">The object that is used to serialize the spans into Thrift</param>
        protected internal JaegerThriftTransport(ISender sender, ISerialization serialization = null)
        {
            _sender = sender;
            _jaegerThriftSerialization = serialization ?? new JaegerThriftSerialization();
        }

        public void Dispose()
        {
            _sender.Dispose();
        }

        /// <summary>
        /// AppendAsync serializes the passed in span into Jaeger Thrift and passes it off
        /// to the sender which will buffer it to be sent. If the buffer is full enough it
        /// will be flushed and the number of spans sent will be returned. If the buffer is
        /// not full enough, nothing will be done and a 0 will be returned to indicate that 0
        /// spans were sent.
        /// </summary>
        /// <param name="span">The span to serialize into Jaeger Thrift and buffer to be sent</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of spans flushed, if any</returns>
        public async Task<int> AppendAsync(IJaegerCoreSpan span, CancellationToken cancellationToken)
        {
            if (_process == null) {
                _process = _jaegerThriftSerialization.BuildJaegerProcessThrift(span.Tracer);
            }

            var jaegerSpan = _jaegerThriftSerialization.BuildJaegerThriftSpan(span);

            return await ProtocolAppendLogicAsync(jaegerSpan, cancellationToken);
        }

        /// <summary>
        /// ProtocolAppendLogicAsync is the internal function to be implemented by concrete
        /// JaegerThriftTransport that handles the logic after AppendAsync serializes a span.
        /// When implementing ProtocolAppendLogicAsync it can be assumed that _process is populated.
        /// </summary>
        /// <param name="span">The span serialized in Jaeger Thrift</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal abstract Task<int> ProtocolAppendLogicAsync(JaegerSpan span, CancellationToken cancellationToken);

        /// <summary>
        /// FlushAsync flushes the sender buffer without checking to see if the sender has
        /// reached the point at which we would normally flush.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            var sentCount = 0;

            try
            {
                sentCount = await _sender.FlushAsync(_process, cancellationToken);
            }
            catch (Exception e)
            {
                throw new SenderException("Failed to flush spans.", e, sentCount);
            }

            return sentCount;
        }

        /// <summary>
        /// CloseAsync handles flushing the buffer of spans to be sent. If the cancellation
        /// is requested on the cancellationToken nothing will be flushed.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<int>(cancellationToken);
            }

            return FlushAsync(cancellationToken);
        }
    }
}
