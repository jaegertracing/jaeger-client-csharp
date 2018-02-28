using System;
using System.Collections.Generic;
using LetsTrace.Util;

namespace LetsTrace.Samplers
{
    // RateLimitingSampler creates a sampler that samples at most maxTracesPerSecond. The distribution of sampled
    // traces follows burstiness of the service, i.e. a service with uniformly distributed requests will have those
    // requests sampled uniformly as well, but if requests are bursty, especially sub-second, then a number of
    // sequential requests can be sampled each second.
    public class RateLimitingSampler : ISampler
    {
        private IRateLimiter _rateLimiter;
        private Dictionary<string, Field> _tags;

        public RateLimitingSampler(double maxTracesPerSecond)
            : this(maxTracesPerSecond, new RateLimiter(maxTracesPerSecond, Math.Max(maxTracesPerSecond, 1.0)))
        {}

        public RateLimitingSampler(double maxTracesPerSecond, IRateLimiter rateLimiter)
        {
            _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
            _tags = new Dictionary<string, Field> {
                { Constants.SamplerTypeTagKey, new Field<string> { Value = Constants.SamplerTypeRateLimiting } },
                { Constants.SamplerParamTagKey, new Field<double> { Value = maxTracesPerSecond } }
            };
        }

        public void Dispose()
        {
            // nothing to do
        }

        public (bool Sampled, IDictionary<string, Field> Tags) IsSampled(TraceId id, string operation)
        {
            return (_rateLimiter.CheckCredit(1.0), _tags);
        }
    }
}
