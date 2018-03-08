using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LetsTrace.Metrics;
using LetsTrace.Samplers.HTTP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LetsTrace.Samplers
{
    public class RemoteControlledSampler : ISampler
    {

        // TODO: Constants!
        private const int DEFAULT_POLLING_INTERVAL_MS = 60000;

        private readonly int _maxOperations = 2000;
        private readonly string _serviceName;
        private readonly ISamplingManager _samplingManager;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly Task _pollTimer;
        private readonly IMetrics _metrics;
        private ISampler _sampler;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private RemoteControlledSampler(string serviceName, ISamplingManager samplingManager, ILoggerFactory loggerFactory, IMetrics metrics, ISampler sampler, int poolingIntervalMs)
        {
            this._serviceName = serviceName;
            this._samplingManager = samplingManager;
            this._loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this._logger = loggerFactory.CreateLogger<RemoteControlledSampler>();
            this._metrics = metrics;
            this._sampler = sampler;
            this._cancellationTokenSource = new CancellationTokenSource();
            this._pollTimer = UpdateSamplerTimer(poolingIntervalMs, _cancellationTokenSource.Token);
        }

        private async Task UpdateSamplerTimer(int poolingIntervalMs, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    UpdateSampler();
                    await Task.Delay(poolingIntervalMs, cancellationToken);
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
        void UpdateSampler()
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
            else
            {
                UpdateRateLimitingOrProbabilisticSampler(response);
            }
        }

        /// <summary>
        /// Replace <see cref="_sampler"/> with a new instance when parameters are updated.
        /// </summary>
        /// <param name="response">Contains either a <see cref="ProbabilisticSampler"/> or <see cref="RateLimitingSampler"/></param>
        private void UpdateRateLimitingOrProbabilisticSampler(SamplingStrategyResponse response)
        {
            ISampler sampler;
            if (response.ProbabilisticSampling != null)
            {
                ProbabilisticSamplingStrategy strategy = response.ProbabilisticSampling;
                sampler = new ProbabilisticSampler(strategy.SamplingRate);
            }
            else if (response.RateLimitingSampling != null)
            {
                RateLimitingSamplingStrategy strategy = response.RateLimitingSampling;
                sampler = new RateLimitingSampler(strategy.MaxTracesPerSecond);
            }
            else
            {
                _metrics.SamplerParsingFailure.Inc(1);
                _logger.LogError("No strategy present in response. Not updating sampler.");
                return;
            }

            lock (this)
            {
                if (!this._sampler.Equals(sampler))
                {
                    this._sampler = sampler;
                    _metrics.SamplerUpdated.Inc(1);
                }
            }
        }

        private void UpdatePerOperationSampler(PerOperationSamplingStrategies samplingParameters)
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
                    _sampler = new PerOperationSampler(_maxOperations, 
                        samplingParameters.DefaultSamplingProbability,
                        samplingParameters.DefaultLowerBoundTracesPerSecond,
                        _loggerFactory);
                }
            }
        }

        public (bool Sampled, IDictionary<string, Field> Tags) IsSampled(TraceId id, string operation)
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

        public sealed class Builder
        {
            private readonly string _serviceName;
            private readonly ISamplingManager _samplingManager;
            private ILoggerFactory _loggerFactory;
            private ISampler _initialSampler;
            private IMetrics _metrics;
            private int _poolingIntervalMs = DEFAULT_POLLING_INTERVAL_MS;

            public Builder(String serviceName, ISamplingManager samplingManager)
            {
                this._serviceName = serviceName;
                this._samplingManager = samplingManager ?? throw new ArgumentNullException(nameof(samplingManager));
            }

            public Builder WithLoggerFactory(ILoggerFactory loggerFactory)
            {
                this._loggerFactory = loggerFactory;
                return this;
            }

            public Builder WithInitialSampler(ISampler initialSampler)
            {
                this._initialSampler = initialSampler;
                return this;
            }

            public Builder WithMetrics(IMetrics metrics)
            {
                this._metrics = metrics;
                return this;
            }

            public Builder WithPollingInterval(int pollingIntervalMs)
            {
                this._poolingIntervalMs = pollingIntervalMs;
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
                return new RemoteControlledSampler(_serviceName, _samplingManager, _loggerFactory, _metrics, _initialSampler, _poolingIntervalMs);
            }
        }
    }
}
