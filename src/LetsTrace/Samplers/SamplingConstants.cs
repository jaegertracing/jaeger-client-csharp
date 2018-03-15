using System;
using System.Collections.Generic;
using System.Text;

namespace LetsTrace.Samplers
{
    public class SamplerConstants
    {
        /// <summary>
        /// Span tag key to describe the type of sampler used on the root span.
        /// </summary>
        public const string SamplerTypeTagKey = "sampler.type";

        /// <summary>
        /// Span tag key to describe the parameter of the sampler used on the root span.
        /// </summary>
        public const string SamplerParamTagKey = "sampler.param";

        /// <summary>
        /// SamplerTypeConst is the type of sampler that always makes the same decision.
        /// </summary>
        public const string SamplerTypeConst = "const";

        /// <summary>
        /// SamplerTypeProbabilistic is the type of sampler that samples traces
        /// with a certain fixed probability.
        /// </summary>
        public const string SamplerTypeProbabilistic = "probabilistic";

        /// <summary>
        /// SamplerTypeRateLimiting is the type of sampler that samples
        /// only up to a fixed number of traces per second.
        /// </summary>
        public const string SamplerTypeRateLimiting = "ratelimiting";

        public const string SamplerTypeLowerBound = "lowerbound";

        public const double DefaultSamplingProbability = 0.001;

        public const int DefaultRemotePollingIntervalMs = 60000;
    }
}
