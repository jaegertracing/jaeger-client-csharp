using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Core.Reporters.Protocols;
using Jaeger.Core.Senders;
using ThriftSpan = Jaeger.Thrift.Span;

namespace Jaeger.Core.Tests.Reporters
{
    /// <summary>
    /// Sender which stores spans in memory. Appending a new span is a blocking operation unless
    /// "permitted". By default <see cref="int.MaxValue"/> "appends" are permitted.
    /// </summary>
    public class InMemorySender : ISender
    {
        private readonly List<ThriftSpan> _appended;
        private readonly List<ThriftSpan> _flushed;
        private readonly List<ThriftSpan> _received;

        // By default, all Append actions are allowed.
        private ManualResetEventSlim _blocker = new ManualResetEventSlim(true);

        public InMemorySender()
        {
            _appended = new List<ThriftSpan>();
            _flushed = new List<ThriftSpan>();
            _received = new List<ThriftSpan>();
        }

        public List<ThriftSpan> GetAppended()
        {
            return new List<ThriftSpan>(_appended);
        }

        public List<ThriftSpan> GetFlushed()
        {
            return new List<ThriftSpan>(_flushed);
        }

        public List<ThriftSpan> GetReceived()
        {
            return new List<ThriftSpan>(_received);
        }

        public Task<int> AppendAsync(Span span, CancellationToken cancellationToken)
        {
            _blocker.Wait();

            ThriftSpan thriftSpan = JaegerThriftSpanConverter.ConvertSpan(span);
            _appended.Add(thriftSpan);
            _received.Add(thriftSpan);
            return Task.FromResult(0);
        }

        public virtual Task<int> FlushAsync(CancellationToken cancellationToken)
        {
            int flushedSpans = _appended.Count;
            _flushed.AddRange(_appended);
            _appended.Clear();

            return Task.FromResult(flushedSpans);
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