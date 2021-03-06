using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jaeger.Exceptions;
using Jaeger.Metrics;
using Jaeger.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jaeger.Reporters
{
    /// <summary>
    /// <see cref="RemoteReporter"/> buffers spans in memory and sends them out of process using <see cref="ISender"/>.
    /// </summary>
    public class RemoteReporter : IReporter
    {
        public const int DefaultMaxQueueSize = 100;
        public static readonly TimeSpan DefaultFlushInterval = TimeSpan.FromSeconds(1);

        private readonly BufferBlock<ICommand> _commandQueue;
        private readonly Task _queueProcessorTask;
        private readonly TimeSpan _flushInterval;
        private readonly Task _flushTask;
        private readonly ISender _sender;
        private readonly IMetrics _metrics;
        private readonly ILogger _logger;

        internal RemoteReporter(ISender sender, TimeSpan flushInterval, int maxQueueSize,
            IMetrics metrics, ILoggerFactory loggerFactory)
        {
            _sender = sender;
            _metrics = metrics;
            _logger = loggerFactory.CreateLogger<RemoteReporter>();
            _commandQueue = new BufferBlock<ICommand>( new DataflowBlockOptions() {
                BoundedCapacity = maxQueueSize
            });

            // start a thread to append spans
            _queueProcessorTask = Task.Run(async () => { await ProcessQueueLoop(); });

            _flushInterval = flushInterval;
            _flushTask = Task.Run(FlushLoop);
        }

        public void Report(Span span)
        {
            // It's better to drop spans, than to block here
            var added = _commandQueue.Post(new AppendCommand(this, span));
            
            if (!added)
            {
                _metrics.ReporterDropped.Inc(1);
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            _commandQueue.Complete();

            try
            {
                // Give processor some time to process any queued commands.

                var cts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    new CancellationTokenSource(10000).Token);
                var cancellationTask = Task.Delay(Timeout.Infinite, cts.Token);

                await Task.WhenAny(_queueProcessorTask, cancellationTask);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex, "Dispose interrupted");
            }
            finally
            {
                try
                {
                    int n = await _sender.CloseAsync(cancellationToken).ConfigureAwait(false);
                    _metrics.ReporterSuccess.Inc(n);
                }
                catch (SenderException ex)
                {
                    _metrics.ReporterFailure.Inc(ex.DroppedSpanCount);
                }
            }
        }

        internal void Flush()
        {
            // to reduce the number of updateGauge stats, we only emit queue length on flush
            _metrics.ReporterQueueLength.Update(_commandQueue.Count);

            // We can safely drop FlushCommand when the queue is full - sender should take care of flushing
            // in such case
            _commandQueue.Post(new FlushCommand(this));
        }

        private async Task FlushLoop()
        {
            // First flush should happen later so we start with the delay
            do
            {
                await Task.Delay(_flushInterval).ConfigureAwait(false);
                Flush();
            }
            while (!_commandQueue.Completion.IsCompleted);
        }

        private async Task ProcessQueueLoop()
        {
            // This blocks until a command is available or IsCompleted=true
            while (await _commandQueue.OutputAvailableAsync())
            {
                try
                {
                    var command = await _commandQueue.ReceiveAsync();
                    await command.ExecuteAsync();
                }
                catch (SenderException ex)
                {
                    _metrics.ReporterFailure.Inc(ex.DroppedSpanCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "QueueProcessor error");
                    // Do nothing, and try again on next command.
                }
            }
        }

        public override string ToString()
        {
            return $"{nameof(RemoteReporter)}(Sender={_sender})";
        }

        /*
         * The code below implements the command pattern. This pattern is useful for
         * situations where multiple threads would need to synchronize on a resource,
         * but are fine with executing sequentially. The advantage is simplified code where
         * tasks are put onto a blocking queue and processed sequentially by another thread.
         */
        public interface ICommand
        {
            Task ExecuteAsync();
        }

        class AppendCommand : ICommand
        {
            private readonly RemoteReporter _reporter;
            private readonly Span _span;

            public AppendCommand(RemoteReporter reporter, Span span)
            {
                _reporter = reporter;
                _span = span;
            }

            public Task ExecuteAsync()
            {
                return _reporter._sender.AppendAsync(_span, CancellationToken.None);
            }
        }

        class FlushCommand : ICommand
        {
            private readonly RemoteReporter _reporter;

            public FlushCommand(RemoteReporter reporter)
            {
                _reporter = reporter;
            }

            public async Task ExecuteAsync()
            {
                int n = await _reporter._sender.FlushAsync(CancellationToken.None).ConfigureAwait(false);
                _reporter._metrics.ReporterSuccess.Inc(n);
            }
        }

        public sealed class Builder
        {
            private ISender _sender;
            private IMetrics _metrics;
            private ILoggerFactory _loggerFactory;
            private TimeSpan _flushInterval = DefaultFlushInterval;
            private int _maxQueueSize = DefaultMaxQueueSize;

            public Builder WithFlushInterval(TimeSpan flushInterval)
            {
                _flushInterval = flushInterval;
                return this;
            }

            public Builder WithMaxQueueSize(int maxQueueSize)
            {
                _maxQueueSize = maxQueueSize;
                return this;
            }

            public Builder WithMetrics(IMetrics metrics)
            {
                _metrics = metrics;
                return this;
            }

            public Builder WithSender(ISender sender)
            {
                _sender = sender;
                return this;
            }

            public Builder WithLoggerFactory(ILoggerFactory loggerFactory)
            {
                _loggerFactory = loggerFactory;
                return this;
            }

            public RemoteReporter Build()
            {
                if (_loggerFactory == null)
                {
                    _loggerFactory = NullLoggerFactory.Instance;
                }
                if (_sender == null)
                {
                    var senderResolver = Configuration.SenderConfiguration.DefaultSenderResolver
                        ?? new SenderResolver(_loggerFactory);
                    _sender = senderResolver.Resolve();
                }
                if (_metrics == null)
                {
                    _metrics = new MetricsImpl(NoopMetricsFactory.Instance);
                }
                return new RemoteReporter(_sender, _flushInterval, _maxQueueSize, _metrics, _loggerFactory);
            }
        }
    }
}