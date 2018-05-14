using OpenTracing;

namespace Jaeger.Core.Propagation
{
    /// <summary>
    /// You should implement this class if you want to add possibility to extract information about
    /// SpanContext that is provided in your custom propagation scheme. Otherwise you should probably use
    /// built-in <see cref="TextMapCodec"/> or <see cref="B3TextMapCodec"/>.
    /// </summary>
    /// <seealso cref="TextMapCodec"/>
    /// <seealso cref="B3TextMapCodec"/>
    /// <seealso cref="ICodec"/>
    public interface IExtractor
    {
        /// <summary>
        /// Called when <see cref="ITracer.Extract{TCarrier}"/> is used. It should handle the logic behind extracting propagation-scheme
        /// specific information from carrier (e.g. http request headers, amqp message headers, etc.).
        /// <para/>
        /// This method must not modify the carrier.
        /// <para/>
        /// All exceptions thrown from this method will be caught and logged on <code>WARN</code> level so
        /// that business code execution isn't affected. If possible, catch implementation specific
        /// exceptions and log more meaningful information.
        /// </summary>
        /// <param name="carrier">Input that you extract Span information from, usually <see cref="OpenTracing.Propagation.ITextMap"/>.</param>
        /// <returns><see cref="SpanContext"/> or <code>null</code> if carrier doesn't contain tracing information, it
        /// is not valid or is incomplete.</returns>
        /// <seealso cref="TextMapCodec"/>
        /// <seealso cref="B3TextMapCodec"/>
        SpanContext Extract(object carrier);
    }
}