using System;

namespace LetsTrace.Transport
{
    // Transport abstracts sending spans along to a tracing specific collector.
    // This is where spans are translated into their target format from the
    // POCO format.
    public interface ITransport : IDisposable
    {
        // Append should translate the span into the format needed by the
        // tracing implementation target. It should add the span to the
        // transporters internal buffer. If the buffer grows larger than a
        // specified size the transporter should use Flush and return the
        // number of spans flushed.
       int Append(ILetsTraceSpan span);
       // Flush sends the internal buffer to the remote server and returns the
       // number of spans sent.
       int Flush();
    }
}