using LetsTrace.Transport;

namespace LetsTrace.Reporters
{
    // TODO: use this to load up spans into a processing queue that will be taken care of by a thread
    public class RemoteReporter : IReporter
    {
        private readonly ITransport _transport;

        public RemoteReporter(ITransport transport)
        {
            _transport = transport;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void Report(ILetsTraceSpan span)
        {
            _transport.Append(span);
        }
    }
}