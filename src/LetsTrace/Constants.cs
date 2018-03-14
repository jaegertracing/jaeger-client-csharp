using System;

namespace LetsTrace
{
    public class Constants
    {
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
    }
}