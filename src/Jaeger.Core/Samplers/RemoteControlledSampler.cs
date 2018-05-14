using System;
using System.Collections.Generic;
using System.Threading;
using Jaeger.Core.Metrics;
using Jaeger.Core.Samplers.HTTP;
using Jaeger.Core.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jaeger.Core.Samplers
{
    public class RemoteControlledSampler : ValueObject, ISampler
    {
        public const string Type = "remote";
        public static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromMinutes(1);

        private readonly object _lock = new object();
        private readonly int _maxOperations = 2000;
        private readonly string _serviceName;
        private readonly ISamplingManager _samplingManager;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly IMetrics _metrics;
        private readonly Timer _pollTimer;

        internal ISampler Sampler { get; private set; }

        private RemoteControlledSampler(Builder builder)
        {
            _serviceName = builder.ServiceName;
            _samplingManager = builder.SamplingManager;
            _loggerFactory = builder.LoggerFactory;
            _logger = _loggerFactory.CreateLogger<RemoteControlledSampler>();
            _metrics = builder.Metrics;
            Sampler = builder.InitialSampler;

            _pollTimer = new Timer(_ => UpdateSampler(), null, TimeSpan.Zero, builder.PollingInterval);
        }

        /// <summary>
        /// Updates <see cref="Sampler"/> to a new sampler when it is different.
        /// </summary>
        internal void UpdateSampler()
        {
            try
            {
                SamplingStrategyResponse response = _samplingManager.GetSamplingStrategyAsync(_serviceName)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                _metrics.SamplerRetrieved.Inc(1);

                if (response.OperationSampling != null)
                {
                    UpdatePerOperationSampler(response.OperationSampling);
                }
                else
                {
                    UpdateRateLimitingOrProbabilisticSampler(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Updating sampler failed");
                _metrics.SamplerQueryFailure.Inc(1);
            }
        }

        /// <summary>
        /// Replace <see cref="Sampler"/> with a new instance when parameters are updated.
        /// </summary>
        /// <param name="response">Response which contains either a <see cref="ProbabilisticSampler"/>
        /// or <see cref="RateLimitingSampler"/>.</param>
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

            lock (_lock)
            {
                if (!Sampler.Equals(sampler))
                {
                    Sampler.Close();
                    Sampler = sampler;
                    _metrics.SamplerUpdated.Inc(1);
                }
            }
        }

        internal void UpdatePerOperationSampler(OperationSamplingParameters samplingParameters)
        {
            lock (_lock)
            {
                if (Sampler is PerOperationSampler sampler)
                {
                    if (sampler.Update(samplingParameters))
                    {
                        _metrics.SamplerUpdated.Inc(1);
                    }
                }
                else
                {
                    Sampler.Close();
                    Sampler = new PerOperationSampler(_maxOperations, samplingParameters, _loggerFactory);
                }
            }
        }

        public SamplingStatus Sample(string operation, TraceId id)
        {
            lock (_lock)
            {
                return Sampler.Sample(operation, id);
            }
        }

        public override string ToString()
        {
            lock (_lock)
            {
                return $"{nameof(RemoteControlledSampler)}({Sampler})";
            }
        }

        public void Close()
        {
            lock (_lock)
            {
                _pollTimer.Dispose();
                Sampler.Close();
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Sampler;
        }

        public sealed class Builder
        {
            internal string ServiceName { get; }
            internal ISamplingManager SamplingManager { get; private set; }
            internal ILoggerFactory LoggerFactory { get; private set; }
            internal ISampler InitialSampler { get; private set; }
            internal IMetrics Metrics { get; private set; }
            internal TimeSpan PollingInterval { get; private set; } = DefaultPollingInterval;

            public Builder(string serviceName)
            {
                ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            }

            public Builder WithSamplingManager(ISamplingManager samplingManager)
            {
                SamplingManager = samplingManager ?? throw new ArgumentNullException(nameof(samplingManager));
                return this;
            }

            public Builder WithLoggerFactory(ILoggerFactory loggerFactory)
            {
                LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
                return this;
            }

            public Builder WithInitialSampler(ISampler initialSampler)
            {
                InitialSampler = initialSampler ?? throw new ArgumentNullException(nameof(initialSampler));
                return this;
            }

            public Builder WithMetrics(IMetrics metrics)
            {
                Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
                return this;
            }

            public Builder WithPollingInterval(TimeSpan pollingIntervalMs)
            {
                PollingInterval = pollingIntervalMs;
                return this;
            }

            public RemoteControlledSampler Build()
            {
                if (LoggerFactory == null)
                {
                    LoggerFactory = NullLoggerFactory.Instance;
                }
                if (InitialSampler == null)
                {
                    InitialSampler = new ProbabilisticSampler();
                }
                if (Metrics == null)
                {
                    Metrics = new MetricsImpl(NoopMetricsFactory.Instance);
                }
                return new RemoteControlledSampler(this);
            }
        }
    }
}
