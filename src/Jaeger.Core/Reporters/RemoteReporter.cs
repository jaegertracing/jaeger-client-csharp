using System;
using System.Threading;
using Jaeger.Core.Exceptions;
using Jaeger.Core.Metrics;
using Jaeger.Core.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jaeger.Core.Reporters
{
    // TODO: use this to load up spans into a processing queue that will be taken care of by a thread
    public class RemoteReporter : IReporter
    {
        internal readonly ITransport _transport;
        internal readonly IMetrics _metrics;

        private readonly ILogger _logger;

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
        public async void Report(IJaegerCoreSpan span)
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
            private readonly ITransport _transport;
            private ILoggerFactory _loggerFactory;
            //private TimeSpan _flushInterval = REMOTE_REPORTER_DEFAULT_FLUSH_INTERVAL_MS;
            //private int _maxQueueSize = REMOTE_REPORTER_DEFAULT_MAX_QUEUE_SIZE;
            private IMetrics _metrics;

            public Builder(ITransport transport)
            {
                this._transport = transport ?? throw new ArgumentNullException(nameof(transport));
            }

            public Builder WithLoggerFactory(ILoggerFactory loggerFactory)
            {
                this._loggerFactory = loggerFactory;
                return this;
            }

            //public Builder WithFlushInterval(TimeSpan flushInterval)
            //{
            //    this._flushInterval = flushInterval;
            //    return this;
            //}

            //public Builder WithMaxQueueSize(int maxQueueSize)
            //{
            //    this._maxQueueSize = maxQueueSize;
            //    return this;
            //}

            public Builder WithMetrics(IMetrics metrics)
            {
                this._metrics = metrics;
                return this;
            }

            public Builder WithMetricsFactory(IMetricsFactory metricsFactory)
            {
                this._metrics = metricsFactory.CreateMetrics();
                return this;
            }

            public RemoteReporter Build()
            {
                if (_loggerFactory == null)
                {
                    _loggerFactory = NullLoggerFactory.Instance;
                }
                if (_metrics == null)
                {
                    _metrics = NoopMetricsFactory.Instance.CreateMetrics();
                }
                return new RemoteReporter(_transport, _loggerFactory/*, _flushInterval, _maxQueueSize*/, _metrics);
            }
        }
    }
}