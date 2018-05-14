namespace Jaeger.Core
{
    public class Constants
    {
        // TODO these should be configurable

        public const string XUberSource = "x-uber-source";

        /// <summary>
        /// Span tag key to describe the type of sampler used on the root span.
        /// </summary>
        public const string SamplerTypeTagKey = "sampler.type";

        /// <summary>
        /// Span tag key to describe the parameter of the sampler used on the root span.
        /// </summary>
        public const string SamplerParamTagKey = "sampler.param";

        /// <summary>
        /// The name of HTTP header or a <see cref="OpenTracing.Propagation.ITextMap"/> carrier key which,
        /// if found in the carrier, forces the trace to be sampled as "debug" trace.
        /// The value of the header is recorded as the tag on the root span, so that the trace
        /// can be found in the UI using this value as a correlation ID.
        /// </summary>
        public const string DebugIdHeaderKey = "jaeger-debug-id";

        /// <summary>
        /// The name of the tag used to report client version.
        /// </summary>
        public const string JaegerClientVersionTagKey = "jaeger.version";

        /// <summary>
        /// The name used to report host name of the process.
        /// </summary>
        public const string TracerHostnameTagKey = "hostname";

        /// <summary>
        /// The name used to report ip of the process.
        /// </summary>
        public const string TracerIpTagKey = "ip";
    }
}