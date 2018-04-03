using System;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocols;
using JaegerProcess = Jaeger.Thrift.Process;
using JaegerSpan = Jaeger.Thrift.Span;

namespace Jaeger.Transport.Thrift.Transport.Sender
{
    // ISender handles the buffer for the Jaeger Transports as well as flusing
    // the buffer to send all spans along to the Jaeger system
    public interface ISender : IDisposable
    {
        ITProtocolFactory ProtocolFactory { get; }
        int BufferItem(JaegerSpan item);
        Task<int> FlushAsync(JaegerProcess process, CancellationToken cancellationToken);
    }
}
