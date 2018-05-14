using System;
using System.Collections.Generic;
using Jaeger.Core.Samplers.HTTP;
using Jaeger.Core.Util;
using Microsoft.Extensions.Logging;

namespace Jaeger.Core.Samplers
{
    /// <summary>
    /// Computes <see cref="Sample"/> using the name of the operation, and maintains a specific
    /// <see cref="GuaranteedThroughputSampler"/> instance for each operation.
    /// </summary>
    public class PerOperationSampler : ValueObject, ISampler
    {
        private readonly object _lock = new object();
        private readonly ILogger<PerOperationSampler> _logger;

        internal int MaxOperations { get; }
        internal Dictionary<string, GuaranteedThroughputSampler> OperationNameToSampler { get; }
        internal ProbabilisticSampler DefaultSampler { get; private set; }
        internal double LowerBound { get; private set; }


        public PerOperationSampler(int maxOperations, OperationSamplingParameters strategies, ILoggerFactory loggerFactory)
            : this(maxOperations,
                new Dictionary<string, GuaranteedThroughputSampler>(),
                new ProbabilisticSampler(strategies.DefaultSamplingProbability),
                strategies.DefaultLowerBoundTracesPerSecond,
                loggerFactory)
        {
            Update(strategies);
        }

        internal PerOperationSampler(
            int maxOperations,
            Dictionary<string, GuaranteedThroughputSampler> samplers,
            ProbabilisticSampler probabilisticSampler,
            double lowerBound,
            ILoggerFactory loggerFactory)
        {
            MaxOperations = maxOperations;
            OperationNameToSampler = samplers ?? new Dictionary<string, GuaranteedThroughputSampler>();
            DefaultSampler = probabilisticSampler ?? throw new ArgumentNullException(nameof(probabilisticSampler));
            LowerBound = lowerBound;
            _logger = loggerFactory?.CreateLogger<PerOperationSampler>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Updates the <see cref="GuaranteedThroughputSampler"/> for each operation.
        /// </summary>
        /// <param name="strategies">The parameters for operation sampling.</param>
        /// <returns><c>true</c>, if any samplers were updated.</returns>
        public bool Update(OperationSamplingParameters strategies)
        {
            lock (_lock)
            {
                var isUpdated = false;

                LowerBound = strategies.DefaultLowerBoundTracesPerSecond;
                ProbabilisticSampler defaultSampler = new ProbabilisticSampler(strategies.DefaultSamplingProbability);

                if (!defaultSampler.Equals(DefaultSampler))
                {
                    DefaultSampler.Close();
                    DefaultSampler = defaultSampler;
                    isUpdated = true;
                }

                foreach (var strategy in strategies.PerOperationStrategies)
                {
                    var operation = strategy.Operation;
                    var samplingRate = strategy.ProbabilisticSampling.SamplingRate;
                    if (OperationNameToSampler.TryGetValue(operation, out var sampler))
                    {
                        isUpdated = sampler.Update(samplingRate, LowerBound) || isUpdated;
                    }
                    else
                    {
                        if (OperationNameToSampler.Count < MaxOperations)
                        {
                            sampler = new GuaranteedThroughputSampler(samplingRate, LowerBound);
                            OperationNameToSampler[operation] = sampler;
                            isUpdated = true;
                        }
                        else
                        {
                            _logger.LogInformation("Exceeded the maximum number of operations {maxOperations} for per operations sampling", MaxOperations);
                        }
                    }
                }

                return isUpdated;
            }
        }

        public SamplingStatus Sample(string operation, TraceId id)
        {
            lock (_lock)
            {
                if (OperationNameToSampler.TryGetValue(operation, out var sampler))
                {
                    return sampler.Sample(operation, id);
                }

                if (OperationNameToSampler.Count < MaxOperations)
                {
                    var newSampler = new GuaranteedThroughputSampler(DefaultSampler.SamplingRate, LowerBound);
                    OperationNameToSampler[operation] = newSampler;
                    return newSampler.Sample(operation, id);
                }

                return DefaultSampler.Sample(operation, id);
            }
        }

        public override string ToString()
        {
            lock (_lock)
            {
                return $"{nameof(PerOperationSampler)}({LowerBound}/{MaxOperations})";
            }
        }

        public void Close()
        {
            lock (_lock)
            {
                DefaultSampler.Close();
                foreach (var sampler in OperationNameToSampler.Values)
                {
                    sampler.Close();
                }
            }
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return DefaultSampler;
            yield return LowerBound;
            yield return MaxOperations;
            foreach (var kvp in OperationNameToSampler)
            {
                yield return kvp;
            }
        }
    }
}
