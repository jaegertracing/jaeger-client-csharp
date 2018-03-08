using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Jaeger.Thrift.Agent;

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
