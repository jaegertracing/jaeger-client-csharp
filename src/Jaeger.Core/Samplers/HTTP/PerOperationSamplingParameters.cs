using System;
using System.Collections.Generic;
using Jaeger.Core.Util;

namespace Jaeger.Core.Samplers.HTTP
{
    public class PerOperationSamplingParameters : ValueObject
    {
        public string Operation { get; }

        public ProbabilisticSamplingStrategy ProbabilisticSampling { get; }

        public PerOperationSamplingParameters(string operation, ProbabilisticSamplingStrategy probabilisticSampling)
        {
            Operation = operation ?? throw new ArgumentNullException(nameof(operation));
            ProbabilisticSampling = probabilisticSampling;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Operation;
            yield return ProbabilisticSampling;
        }
    }
}