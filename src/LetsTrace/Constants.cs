using LetsTrace.Propagation;

namespace LetsTrace
{
    public class Constants
    {
        public const string TraceContextHeaderName = "X-LetsTrace-Trace-Context";
        public const string TraceBaggageHeaderPrefix = "X-LetsTrace-Baggage";

        // SamplerTypeTagKey reports which sampler was used on the root span.
        public const string SamplerTypeTagKey = "sampler.type";

        // SamplerParamTagKey reports the parameter of the sampler, like sampling probability.
        public const string SamplerParamTagKey = "sampler.param";

        // SamplerTypeConst is the type of sampler that always makes the same decision.
        public const string SamplerTypeConst = "const";
    }
}