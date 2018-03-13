using OpenTracing.Propagation;

namespace LetsTrace.Propagation
{
    public class TextMapPropagationRegistry : PropagationRegistry
    {
        internal static readonly IHeadersConfig DefaultHeadersConfig = new HeadersConfig(Constants.TRACE_CONTEXT_HEADER_NAME, Constants.TRACE_BAGGAGE_HEADER_PREFIX);

        public TextMapPropagationRegistry() : this(DefaultHeadersConfig)
        {
        }

        public TextMapPropagationRegistry(IHeadersConfig headersConfig)
        {
            var textPropagator = TextMapPropagator.NewTextMapPropagator(headersConfig);
            AddCodec(BuiltinFormats.TextMap, textPropagator, textPropagator);

            var httpHeaderPropagator = TextMapPropagator.NewHTTPHeaderPropagator(headersConfig);
            AddCodec(BuiltinFormats.HttpHeaders, httpHeaderPropagator, httpHeaderPropagator);
        }
    }
}
