namespace Jaeger.Core
{
    public class Constants
    {
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

        public const string TraceContextHeaderName = "uber-trace-id";

        public const string TraceBaggageHeaderPrefix = "uberctx-";
    }
}