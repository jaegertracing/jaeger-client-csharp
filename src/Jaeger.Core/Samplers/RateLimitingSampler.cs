using System;
using System.Collections.Generic;
using Jaeger.Core.Util;

namespace Jaeger.Core.Samplers
{
    /// <summary>
    /// <see cref="RateLimitingSampler"/> creates a sampler that samples at most maxTracesPerSecond. The distribution of sampled
    /// traces follows burstiness of the service, i.e. a service with uniformly distributed requests will have those
    /// requests sampled uniformly as well, but if requests are bursty, especially sub-second, then a number of
    /// sequential requests can be sampled each second.
    /// </summary>
    public class RateLimitingSampler : ValueObject, ISampler
    {
        public const string Type = "ratelimiting";

        private readonly RateLimiter _rateLimiter;
        private readonly IReadOnlyDictionary<string, object> _tags;

        public double MaxTracesPerSecond { get; }

        public RateLimitingSampler(double maxTracesPerSecond)
        {
            MaxTracesPerSecond = maxTracesPerSecond;
            _rateLimiter = new RateLimiter(maxTracesPerSecond, Math.Max(maxTracesPerSecond, 1.0));

            _tags = new Dictionary<string, object> {
                { Constants.SamplerTypeTagKey, Type },
                { Constants.SamplerParamTagKey, maxTracesPerSecond }
            };
        }

        public SamplingStatus Sample(string operation, TraceId id)
        {
            return new SamplingStatus(_rateLimiter.CheckCredit(1.0), _tags);
        }

        public override string ToString()
        {
            return $"{nameof(RateLimitingSampler)}({MaxTracesPerSecond})";
        }

        public void Close()
        {
            // nothing to do
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return MaxTracesPerSecond;
        }
    }
}
