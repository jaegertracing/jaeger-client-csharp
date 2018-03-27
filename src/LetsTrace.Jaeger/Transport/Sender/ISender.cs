using System;
using System.Threading;
using System.Threading.Tasks;

using JaegerProcess = Jaeger.Thrift.Process;
using JaegerSpan = Jaeger.Thrift.Span;

namespace LetsTrace.Jaeger.Transport.Sender
{
    // ISender handles the buffer for the Jaeger Transports as well as flusing
    // the buffer to send all spans along to the Jaeger system
    public interface ISender : IDisposable
    {
        int BufferItem(JaegerSpan item);
        Task<int> FlushAsync(JaegerProcess process, CancellationToken cancellationToken);
    }
}
