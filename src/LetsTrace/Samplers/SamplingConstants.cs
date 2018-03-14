using System;
using System.Collections.Generic;
using System.Text;

namespace LetsTrace.Samplers
{
    public class SamplingConstants
    {
        /// <summary>
        /// Span tag key to describe the type of sampler used on the root span.
        /// </summary>
        public const string SAMPLER_TYPE_TAG_KEY = "sampler.type";

        /// <summary>
        /// Span tag key to describe the parameter of the sampler used on the root span.
        /// </summary>
        public const string SAMPLER_PARAM_TAG_KEY = "sampler.param";

        /// <summary>
        /// SamplerTypeConst is the type of sampler that always makes the same decision.
        /// </summary>
        public const string SAMPLER_TYPE_CONST = "const";

        /// <summary>
        /// SamplerTypeProbabilistic is the type of sampler that samples traces
        /// with a certain fixed probability.
        /// </summary>
        public const string SAMPLER_TYPE_PROBABILISTIC = "probabilistic";

        /// <summary>
        /// SamplerTypeRateLimiting is the type of sampler that samples
        /// only up to a fixed number of traces per second.
        /// </summary>
        public const string SAMPLER_TYPE_RATE_LIMITING = "ratelimiting";

        /// <summary>
        /// TODO
        /// </summary>
        public const string SAMPLER_TYPE_LOWERBOUND = "lowerbound";

        public const double DEFAULT_SAMPLING_PROBABILITY = 0.001;

        public const int DEFAULT_REMOTE_POLLING_INTERVAL_MS = 60000;
    }
}
