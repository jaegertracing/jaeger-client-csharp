using OpenTracing.Propagation;

namespace Jaeger.Core.Propagation
{
    public class TextMapPropagationRegistry : PropagationRegistry
    {
        internal static readonly IHeadersConfig DefaultHeadersConfig = new HeadersConfig(Constants.TraceContextHeaderName, Constants.TraceBaggageHeaderPrefix);

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
