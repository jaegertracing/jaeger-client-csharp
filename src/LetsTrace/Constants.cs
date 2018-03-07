using LetsTrace.Propagation;

namespace LetsTrace
{
    public class Constants
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
        /// The name of the tag used to report client version.
        /// </summary>
        public const string LETSTRACE_CLIENT_VERSION_TAG_KEY = "letstrace.version";

        /// <summary>
        /// The name used to report host name of the process.
        /// </summary>
        public const string TRACER_HOSTNAME_TAG_KEY = "hostname";

        /// <summary>
        /// The name used to report ip of the process.
        /// </summary>
        public const string TRACER_IP_TAG_KEY = "ip";

        /// <summary>
        /// TODO
        /// </summary>
        public const string TRACE_CONTEXT_HEADER_NAME = "X-LetsTrace-Trace-Context";

        /// <summary>
        /// TODO
        /// </summary>
        public const string TRACE_BAGGAGE_HEADER_PREFIX = "X-LetsTrace-Baggage";

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
    }
}