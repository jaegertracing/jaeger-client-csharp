namespace Jaeger.Core.Samplers
{
    /// <summary>
    /// <see cref="ISampler"/> is responsible for deciding if a new trace should be sampled and captured for storage.
    /// </summary>
    public interface ISampler
    {
        /// <summary>
        /// Returns whether or not the new trace should be sampled.
        /// </summary>
        /// <param name="operation">The operation name set on the span.</param>
        /// <param name="id">The traceId on the span.</param>
        SamplingStatus Sample(string operation, TraceId id);

        /// <summary>
        /// Release any resources used by the sampler.
        /// </summary>
        /// <remarks>
        /// We don't use <see cref="System.IDisposable"/> because the <see cref="Tracer"/> should
        /// be able to close the sampler. If we would use <see cref="System.IDisposable"/> then
        /// the <see cref="Tracer"/> would call Dispose on a member it did not create itself.
        /// </remarks>
        void Close();
    }
}
