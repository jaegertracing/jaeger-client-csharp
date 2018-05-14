using OpenTracing;

namespace Jaeger.Core.Propagation
{
    /// <summary>
    /// You should implement this class if you want to add possibility to inject information about
    /// <see cref="SpanContext"/> that is passed between services in your custom propagation scheme. Otherwise you
    /// should probably use built-in <see cref="TextMapCodec"/> or <see cref="B3TextMapCodec"/>.
    /// </summary>
    /// <seealso cref="TextMapCodec"/>
    /// <seealso cref="B3TextMapCodec"/>
    /// <seealso cref="ICodec"/>
    public interface IInjector
    {
        /// <summary>
        /// Called when <see cref="ITracer.Inject{TCarrier}"/> is used. It should handle the logic behind injecting propagation scheme
        /// specific information into the carrier (e.g. http request headers, amqp message headers,
        /// etc.).
        /// <para/>
        /// All exceptions thrown from this method will be caught and logged on <code>ERROR</code> level so
        /// that business code execution isn't affected. If possible, catch implementation specific
        /// exceptions and log more meaningful information.
        /// </summary>
        /// <param name="spanContext">Span context that should be used to pass trace information with the carrier.</param>
        /// <param name="carrier">Holder of data that is used to pass tracing information between processes.</param>
        /// <seealso cref="TextMapCodec"/>
        /// <seealso cref="B3TextMapCodec"/>
        void Inject(SpanContext spanContext, object carrier);
    }
}