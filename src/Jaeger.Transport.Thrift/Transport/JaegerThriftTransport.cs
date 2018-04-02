using System;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Core;
using Jaeger.Core.Exceptions;
using Jaeger.Core.Transport;
using Jaeger.Transport.Thrift.Serialization;
using Jaeger.Transport.Thrift.Transport.Sender;
using Thrift.Protocols;
using JaegerProcess = Jaeger.Thrift.Process;

namespace Jaeger.Transport.Thrift.Transport
{
    public abstract class JaegerThriftTransport : ITransport
    {
        internal const int DefaultBufferSize = 10;
        internal readonly int BufferSize;

        private readonly ISerialization _jaegerThriftSerialization;
        protected readonly ITProtocolFactory _protocolFactory;
        protected readonly ISender _sender;
        protected JaegerProcess _process;

        protected internal JaegerThriftTransport(ITProtocolFactory protocolFactory, ISender sender, ISerialization serialization = null, int bufferSize = 0)
        {
            if (bufferSize <= 0)
            {
                bufferSize = DefaultBufferSize;
            }

            _protocolFactory = protocolFactory;
            _sender = sender;
            BufferSize = bufferSize;
            _jaegerThriftSerialization = serialization ?? new JaegerThriftSerialization();
        }

        public void Dispose()
        {
            _sender.Dispose();
        }

        public async Task<int> AppendAsync(IJaegerCoreSpan span, CancellationToken canellationToken)
        {
            if (_process == null) {
                _process = _jaegerThriftSerialization.BuildJaegerProcessThrift(span.Tracer);
            }
            var jaegerSpan = _jaegerThriftSerialization.BuildJaegerThriftSpan(span);

            var curBuffCount = _sender.BufferItem(jaegerSpan);

            if (curBuffCount >= BufferSize) {
                return await _sender.FlushAsync(_process, canellationToken);
            }

            return 0;
        }

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
