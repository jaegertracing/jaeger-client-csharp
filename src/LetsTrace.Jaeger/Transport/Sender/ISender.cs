using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using JaegerProcess = Jaeger.Thrift.Process;
using JaegerSpan = Jaeger.Thrift.Span;

namespace LetsTrace.Jaeger.Transport.Sender
{
    // ISender allows for the testing of a transport in it's queuing and sending
    public interface ISender : IDisposable
    {
        int BufferItem(JaegerSpan item);
        Task<int> FlushAsync(JaegerProcess process, CancellationToken cancellationToken);
    }
}
