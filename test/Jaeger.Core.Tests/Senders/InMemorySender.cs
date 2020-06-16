using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Senders;

namespace Jaeger.Core.Tests.Senders
{
    /// <summary>
    /// Sender which stores spans in memory. Appending a new span is a blocking operation unless
    /// "permitted". By default <see cref="int.MaxValue"/> "appends" are permitted.
    /// </summary>
    public class InMemorySender : ISender
    {
        private readonly List<Span> _appended;
        private readonly List<Span> _flushed;
        private readonly List<Span> _received;

        // By default, all Append actions are allowed.
        private ManualResetEventSlim _blocker = new ManualResetEventSlim(true);

        public InMemorySender()
        {
            _appended = new List<Span>();
            _flushed = new List<Span>();
            _received = new List<Span>();
        }

        public List<Span> GetAppended()
        {
            return new List<Span>(_appended);
        }

        public List<Span> GetFlushed()
        {
            return new List<Span>(_flushed);
        }

        public List<Span> GetReceived()
        {
            return new List<Span>(_received);
        }

        public async Task<int> AppendAsync(Span span, CancellationToken cancellationToken)
        {
            _blocker.Wait(cancellationToken);
            await Task.Delay(1);

            lock (_appended)
            {
                _appended.Add(span);
                _received.Add(span);
            }
            return 0;
        }

        public virtual async Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(1);

            int flushedSpans;
            lock (_appended )
            {
                flushedSpans = _appended.Count;
                _flushed.AddRange(_appended);
                _appended.Clear();
            }

            return flushedSpans;
        }

        public Task<int> CloseAsync(CancellationToken cancellationToken)
        {
            return FlushAsync(cancellationToken);
        }

        public void BlockAppend()
        {
            _blocker.Reset();
        }

        public void AllowAppend()
        {
            _blocker.Set();
        }
    }
}