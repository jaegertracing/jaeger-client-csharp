using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocols;
using JaegerSpan = Jaeger.Thrift.Span;
using JaegerProcess = Jaeger.Thrift.Process;

namespace Jaeger.Transport.Thrift.Transport.Sender
{
    public abstract class JaegerSender : ISender
    {
        protected List<JaegerSpan> _buffer = new List<JaegerSpan>();
        protected JaegerProcess _process;
        protected readonly ITProtocolFactory _protocolFactory;

        protected internal JaegerSender(ITProtocolFactory protocolFactory)
        {
            _protocolFactory = protocolFactory;
        }

        public void Dispose()
        {
            FlushAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public int BufferItem(JaegerSpan item)
        {
            lock (_buffer)
            {
                _buffer.Add(item);
                return _buffer.Count;
            }
        }

        public Task<int> FlushAsync(JaegerProcess process, CancellationToken cancellationToken)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
            return FlushAsync(cancellationToken);
        }

        private async Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            List<JaegerSpan> sendBuffer;

            lock (_buffer)
            {
                sendBuffer = new List<JaegerSpan>(_buffer);
                _buffer.Clear();
            }
            
            if (sendBuffer.Count <= 0)
            {
                return 0;
            }

            return await SendAsync(sendBuffer, cancellationToken);
        }

        protected abstract Task<int> SendAsync(List<JaegerSpan> spans, CancellationToken cancellationToken);
    }
}
