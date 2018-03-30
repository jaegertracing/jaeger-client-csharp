using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Core.Metrics;
using Jaeger.Core.Samplers.HTTP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jaeger.Core.Samplers
{
    public class RemoteControlledSampler : ISampler
    {
        internal readonly ISamplingManager _samplingManager;

        private readonly int _maxOperations = 2000;
        private readonly string _serviceName;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IMetrics _metrics;
        private ISampler _sampler;
        private readonly ISamplerFactory _samplerFactory;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _pollTimer;

        private RemoteControlledSampler(
            string serviceName,
            ISamplingManager samplingManager,
            ILoggerFactory loggerFactory,
            IMetrics metrics,
            ISampler sampler,
            int pollingIntervalMs)
        : this(serviceName, samplingManager, loggerFactory, metrics, sampler, new SamplerFactory(), pollingIntervalMs, PollTimer)
        {}

        internal RemoteControlledSampler(
            string serviceName,
            ISamplingManager samplingManager,
            ILoggerFactory loggerFactory,
            IMetrics metrics,
            ISampler sampler,
            ISamplerFactory samplerFactory,
            int pollingIntervalMs,
            Func<Action, int, CancellationToken, Task> pollTimer)
        {
            _serviceName = serviceName;
            _samplingManager = samplingManager;
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<RemoteControlledSampler>();
            _metrics = metrics;
            _sampler = sampler;
            _samplerFactory = samplerFactory;
            _cancellationTokenSource = new CancellationTokenSource();
            _pollTimer = pollTimer(UpdateSampler, pollingIntervalMs, _cancellationTokenSource.Token);
        }

        internal static async Task PollTimer(Action updateFunc, int pollingIntervalMs, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    updateFunc();
                    await Task.Delay(pollingIntervalMs, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _pollTimer.Wait();
        }

        /// <summary>
        /// Updates <see cref="_sampler"/> to a new sampler when it is different.
        /// </summary>
        internal void UpdateSampler()
        {
            SamplingStrategyResponse response;
            try
            {
                response = _samplingManager.GetSamplingStrategy(_serviceName);
                _metrics.SamplerRetrieved.Inc(1);
            }
            catch (Exception)
            {
                _metrics.SamplerQueryFailure.Inc(1);
                return;
            }

            if (response.OperationSampling != null)
            {
                UpdatePerOperationSampler(response.OperationSampling);
            }
            else if (response.ProbabilisticSampling != null)
            {
                UpdateProbabilisticSampler(response.ProbabilisticSampling);
            }
            else if (response.RateLimitingSampling != null)
            {
                UpdateRateLimitingSampler(response.RateLimitingSampling);
            }
            else
            {
                _metrics.SamplerParsingFailure.Inc(1);
                _logger.LogError("No strategy present in response. Not updating sampler.");
            }
        }

        internal void SetSamplerIfNotTheSame(ISampler newSampler)
        {
            lock (this)
            {
                if (!_sampler.Equals(newSampler))
                {
                    _sampler = newSampler;
                    _metrics.SamplerUpdated.Inc(1);
                }
            }
        }

        /// <summary>
        /// Replace <see cref="_sampler"/> with a new instance when parameters are updated.
        /// </summary>
        /// <param name="strategy"><see cref="ProbabilisticSamplingStrategy"/></param>
        internal void UpdateProbabilisticSampler(ProbabilisticSamplingStrategy strategy)
        {
            var sampler = _samplerFactory.NewProbabilisticSampler(strategy.SamplingRate);

            SetSamplerIfNotTheSame(sampler);
        }

        /// <summary>
        /// Replace <see cref="_sampler"/> with a new instance when parameters are updated.
        /// </summary>
        /// <param name="strategy"><see cref="RateLimitingSamplingStrategy"/></param>
        internal void UpdateRateLimitingSampler(RateLimitingSamplingStrategy strategy)
        {
            var sampler = _samplerFactory.NewRateLimitingSampler(strategy.MaxTracesPerSecond);

            SetSamplerIfNotTheSame(sampler);
        }

        internal void UpdatePerOperationSampler(PerOperationSamplingStrategies samplingParameters)
        {
            lock (this)
            {
                if (_sampler is PerOperationSampler sampler)
                {
                    if (sampler.Update(samplingParameters))
                    {
                        _metrics.SamplerUpdated.Inc(1);
                    }
                }
                else
                {
                    _sampler = _samplerFactory.NewPerOperationSampler(_maxOperations,
                        samplingParameters.DefaultSamplingProbability,
                        samplingParameters.DefaultLowerBoundTracesPerSecond,
                        _loggerFactory);
                }
            }
        }

        public (bool Sampled, Dictionary<string, object> Tags) IsSampled(TraceId id, string operation)
        {
            lock (this)
            {
                return _sampler.IsSampled(id, operation);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is RemoteControlledSampler remoteControlledSampler)
            {
                lock (remoteControlledSampler)
                {
                    lock (this)
                    {
                        return _sampler.Equals(remoteControlledSampler._sampler);
                    }
                }
            }

            return false;
        }
        public override int GetHashCode()
        {
            // TODO Do we need special handling here? (see Equals handling)
            return base.GetHashCode();
        }

        public sealed class Builder
        {
            private readonly string _serviceName;
            private readonly ISamplingManager _samplingManager;
            private ILoggerFactory _loggerFactory;
            private ISampler _initialSampler;
            private IMetrics _metrics;
            private int _pollingIntervalMs = SamplerConstants.DefaultRemotePollingIntervalMs;

            public Builder(string serviceName, ISamplingManager samplingManager)
            {
                _serviceName = serviceName;
                _samplingManager = samplingManager ?? throw new ArgumentNullException(nameof(samplingManager));
            }

            public Builder WithLoggerFactory(ILoggerFactory loggerFactory)
            {
                _loggerFactory = loggerFactory;
                return this;
            }

            public Builder WithInitialSampler(ISampler initialSampler)
            {
                _initialSampler = initialSampler;
                return this;
            }

            public Builder WithMetrics(IMetrics metrics)
            {
                _metrics = metrics;
                return this;
            }

            public Builder WithPollingInterval(int pollingIntervalMs)
            {
                _pollingIntervalMs = pollingIntervalMs;
                return this;
            }

            public RemoteControlledSampler Build()
            {
                if (_loggerFactory == null)
                {
                    _loggerFactory = NullLoggerFactory.Instance;
                }
                if (_initialSampler == null)
                {
                    _initialSampler = new ProbabilisticSampler();
                }
                if (_metrics == null)
                {
                    _metrics = NoopMetricsFactory.Instance.CreateMetrics();
                }
                return new RemoteControlledSampler(_serviceName, _samplingManager, _loggerFactory, _metrics, _initialSampler, _pollingIntervalMs);
            }
        }
    }
}
