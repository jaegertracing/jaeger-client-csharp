using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LetsTrace.Samplers.HTTP;
using Microsoft.Extensions.Logging;
using SamplerDictionary = System.Collections.Generic.Dictionary<string, LetsTrace.Samplers.IGuaranteedThroughputProbabilisticSampler>;

namespace LetsTrace.Samplers
{
    // PerOperationSampler is a sampler that uses the name of the operation to
    // maintain a specific GuaranteedThroughputProbabilisticSampler instance
    // for each operation
    public class PerOperationSampler : ISampler
    {
        private readonly int _maxOperations;
        private readonly double _samplingRate;
        private double _lowerBound;
        private readonly ILogger<PerOperationSampler> _logger;
        private readonly ISamplerFactory _factory;
        private readonly SamplerDictionary _samplers = new SamplerDictionary();
        private ISampler _defaultSampler;

        public PerOperationSampler(int maxOperations, double samplingRate, double lowerBound, ILoggerFactory loggerFactory)
            : this(maxOperations, samplingRate, lowerBound, loggerFactory, new SamplerFactory())
        {}

        internal PerOperationSampler(int maxOperations, double samplingRate, double lowerBound, ILoggerFactory loggerFactory, ISamplerFactory samplerFactory)
        {
            _maxOperations = maxOperations;
            _samplingRate = samplingRate;
            _lowerBound = lowerBound;
            _logger = loggerFactory?.CreateLogger<PerOperationSampler>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            _factory = samplerFactory ?? throw new ArgumentNullException(nameof(samplerFactory));
            _defaultSampler = _factory.NewProbabilisticSampler(_samplingRate);
        }

        public void Dispose()
        {
            foreach(var samplerKV in _samplers)
            {
                samplerKV.Value.Dispose();
            }
        }

        /// <summary>
        /// Updates the GuaranteedThroughputSampler for each operation.
        /// </summary>
        /// <param name="strategies">The parameters for operation sampling</param>
        /// <returns>true, iff any samplers were updated</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Update(PerOperationSamplingStrategies strategies)
        {
            var isUpdated = false;

            _lowerBound = strategies.DefaultLowerBoundTracesPerSecond;

            var defaultSampler = _factory.NewProbabilisticSampler(strategies.DefaultSamplingProbability);
            if (!defaultSampler.Equals(_defaultSampler))
            {
                _defaultSampler = defaultSampler;
                isUpdated = true;
            }

            foreach (var strategy in strategies.PerOperationStrategies)
            {
                var operation = strategy.Operation;
                var samplingRate = strategy.ProbabilisticSampling.SamplingRate;
                if (_samplers.TryGetValue(operation, out var sampler))
                {
                    isUpdated = sampler.Update(samplingRate, _lowerBound) || isUpdated;
                }
                else
                {
                    if (_samplers.Count < _maxOperations)
                    {
                        sampler = (IGuaranteedThroughputProbabilisticSampler)_factory.NewGuaranteedThroughputProbabilisticSampler(samplingRate, _lowerBound);
                        _samplers.Add(operation, sampler);
                        isUpdated = true;
                    }
                    else
                    {
                        _logger.LogInformation($"Exceeded the maximum number of operations ({_maxOperations}) for per operations sampling");
                    }
                }
            }

            return isUpdated;
        }

        public (bool Sampled, IDictionary<string, Field> Tags) IsSampled(TraceId id, string operation)
        {
            var operationKey = operation.ToLower();

            if (_samplers.TryGetValue(operationKey, out var sampler)) {
                return sampler.IsSampled(id, operation);
            }

            if (_samplers.Count >= _maxOperations) {
                return _defaultSampler.IsSampled(id, operation);
            }

            var newSampler = (IGuaranteedThroughputProbabilisticSampler)_factory.NewGuaranteedThroughputProbabilisticSampler(_samplingRate, _lowerBound);
            _samplers[operationKey] = newSampler;
            return newSampler.IsSampled(id, operation);
        }
    }
}
