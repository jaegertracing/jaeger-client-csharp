using System;
using System.Threading;
using LetsTrace.Exceptions;
using LetsTrace.Metrics;
using LetsTrace.Transport;
using Microsoft.Extensions.Logging;

namespace LetsTrace.Reporters
{
    // TODO: use this to load up spans into a processing queue that will be taken care of by a thread
    public class RemoteReporter : IReporter
    {
        // TODO: Constants
        public static readonly TimeSpan REMOTE_REPORTER_DEFAULT_FLUSH_INTERVAL_MS = TimeSpan.FromMilliseconds(100);
        public const int REMOTE_REPORTER_DEFAULT_MAX_QUEUE_SIZE = 100;

        private readonly ITransport _transport;
        private readonly ILogger _logger;
        private readonly IMetrics _metrics;

        private RemoteReporter(ITransport transport, ILoggerFactory loggerFactory, IMetrics metrics)
        {
            _transport = transport;
            _logger = loggerFactory.CreateLogger<RemoteReporter>();
            _metrics = metrics;
        }

        public async void Dispose()
        {
            try
            {
                int n = await _transport.CloseAsync(CancellationToken.None).ConfigureAwait(false);
                _metrics.ReporterSuccess.Inc(n);
            }
            catch (SenderException e)
            {
                _logger.LogError(e, "Unable to cleanly close RemoteReporter");
                _metrics.ReporterFailure.Inc(e.DroppedSpans);
            }
        }

        // TODO: Make async!
        public async void Report(ILetsTraceSpan span)
        {
            try
            {
                // TODO: This Task should be queued and be processed in a separate thread
                await _transport.AppendAsync(span, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to report span in RemoteReporter");
                _metrics.ReporterDropped.Inc(1);
            }
        }

        // TODO: Make async!
        private async void Flush()
        {
            try
            {
                // TODO: Not exposed, this should be the list of unprocessed Report calls
                //_metrics.ReporterQueueLength.Update(_commandQueue.Count);
                int n = await _transport.FlushAsync(CancellationToken.None).ConfigureAwait(false);
                _metrics.ReporterSuccess.Inc(n);
            }
            catch (SenderException e)
            {
                _logger.LogError(e, "Unable to flush RemoteReporter");
                _metrics.ReporterFailure.Inc(e.DroppedSpans);
            }
        }

        public class Builder
        {
            private readonly ITransport transport;
            private ILoggerFactory _loggerFactory;
            //private TimeSpan flushInterval = REMOTE_REPORTER_DEFAULT_FLUSH_INTERVAL_MS;
            //private int maxQueueSize = REMOTE_REPORTER_DEFAULT_MAX_QUEUE_SIZE;
            private IMetrics metrics;

            public Builder(ITransport transport)
            {
                this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            }

            public Builder WithLoggerFactory(ILoggerFactory loggerFactory)
            {
                this._loggerFactory = loggerFactory;
                return this;
            }

            //public Builder WithFlushInterval(TimeSpan flushInterval)
            //{
            //    this.flushInterval = flushInterval;
            //    return this;
            //}

            //public Builder WithMaxQueueSize(int maxQueueSize)
            //{
            //    this.maxQueueSize = maxQueueSize;
            //    return this;
            //}

            public Builder WithMetrics(IMetrics metrics)
            {
                this.metrics = metrics;
                return this;
            }

            public RemoteReporter Build()
            {
                if (metrics == null)
                {
                    metrics = NoopMetricsFactory.Instance.CreateMetrics();
                }
                return new RemoteReporter(transport, _loggerFactory/*, flushInterval, maxQueueSize*/, metrics);
            }
        }
    }
}