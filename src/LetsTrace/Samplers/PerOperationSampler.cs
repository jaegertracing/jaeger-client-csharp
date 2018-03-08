using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LetsTrace.Samplers.HTTP;

namespace LetsTrace.Samplers
{
    // PerOperationSampler is a sampler that uses the name of the operation to
    // maintain a specific GuaranteedThroughputProbabilisticSampler instance
    // for each operation
    public class PerOperationSampler : ISampler
    {
        private int _maxOperations;
        private double _samplingRate;
        private double _lowerBound;
        private Dictionary<string, IGuaranteedThroughputProbabilisticSampler> _samplers = new Dictionary<string, IGuaranteedThroughputProbabilisticSampler>();
        private ISampler _defaultSampler;
        private ISamplerFactory _factory;

        public PerOperationSampler(int maxOperations, double samplingRate, double lowerBound)
            : this(maxOperations, samplingRate, lowerBound, new SamplerFactory())
        {}

        internal PerOperationSampler(int maxOperations, double samplingRate, double lowerBound, ISamplerFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _maxOperations = maxOperations;
            _samplingRate = samplingRate;
            _lowerBound = lowerBound;
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
            bool isUpdated = false;

            _lowerBound = strategies.DefaultLowerBoundTracesPerSecond;

            var defaultSampler = _factory.NewProbabilisticSampler(strategies.DefaultSamplingProbability);
            if (!defaultSampler.Equals(_defaultSampler))
            {
                this._defaultSampler = defaultSampler;
                isUpdated = true;
            }

            foreach (var strategy in strategies.PerOperationStrategies)
            {
                String operation = strategy.Operation;
                double samplingRate = strategy.ProbabilisticSampling.SamplingRate;
                if (_samplers.TryGetValue(operation, out var sampler))
                {
                    isUpdated = sampler.Update(samplingRate, _lowerBound) || isUpdated;
                }
                else
                {
                    if (_samplers.Count < _maxOperations)
                    {
                        sampler = _factory.NewGuaranteedThroughputProbabilisticSampler(samplingRate, _lowerBound);
                        _samplers.Add(operation, sampler);
                        isUpdated = true;
                    }
                    else
                    {
                        // TODO: Use ILogger log.info
                        Console.WriteLine("Exceeded the maximum number of operations ({0}) for per operations sampling",
                            _maxOperations);
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

            var newSampler = _factory.NewGuaranteedThroughputProbabilisticSampler(_samplingRate, _lowerBound);
            _samplers[operationKey] = newSampler;
            return newSampler.IsSampled(id, operation);
        }
    }
}
