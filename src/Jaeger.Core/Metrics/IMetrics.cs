namespace Jaeger.Core.Metrics
{
    public interface IMetrics
    {
        // Note: Java does not have this interface. It instead uses a concrete "Metrics" class.
        // However, in C# a type can not have the same name as the namespace so using "Metrics" as a class name is not possible.
        // To overcome this, we use a combination of "IMetrics" and "MetricsImpl".

        /// <summary>
        /// Number of traces started by this tracer as sampled.
        /// </summary>
        ICounter TraceStartedSampled { get; }

        /// <summary>
        /// Number of traces started by this tracer as not sampled.
        /// </summary>
        ICounter TraceStartedNotSampled { get; }

        /// <summary>
        /// Number of externally started sampled traces this tracer joined.
        /// </summary>
        ICounter TracesJoinedSampled { get; }

        /// <summary>
        /// Number of externally started not-sampled traces this tracer joined.
        /// </summary>
        ICounter TracesJoinedNotSampled { get; }

        /// <summary>
        /// Number of sampled spans started by this tracer.
        /// </summary>
        ICounter SpansStartedSampled { get; }

        /// <summary>
        /// Number of unsampled spans started by this tracer.
        /// </summary>
        ICounter SpansStartedNotSampled { get; }

        /// <summary>
        /// Number of spans finished by this tracer.
        /// </summary>
        ICounter SpansFinished { get; }

        /// <summary>
        /// Number of errors decoding tracing context.
        /// </summary>
        ICounter DecodingErrors { get; }

        /// <summary>
        /// Number of spans successfully reported.
        /// </summary>
        ICounter ReporterSuccess { get; }

        /// <summary>
        /// Number of spans not reported due to a Sender failure.
        /// </summary>
        ICounter ReporterFailure { get; }

        /// <summary>
        /// Number of spans dropped due to internal queue overflow.
        /// </summary>
        ICounter ReporterDropped { get; }

        /// <summary>
        /// Current number of spans in the reporter queue.
        /// </summary>
        IGauge ReporterQueueLength { get; }

        /// <summary>
        /// Number of times the Sampler succeeded to retrieve sampling strategy.
        /// </summary>
        ICounter SamplerRetrieved { get; }

        /// <summary>
        /// Number of times the Sampler failed to retrieve sampling strategy.
        /// </summary>
        ICounter SamplerQueryFailure { get; }

        /// <summary>
        /// Number of times the Sampler succeeded to retrieve and update sampling strategy.
        /// </summary>
        ICounter SamplerUpdated { get; }

        /// <summary>
        /// Number of times the Sampler failed to update sampling strategy.
        /// </summary>
        ICounter SamplerParsingFailure { get; }

        /// <summary>
        /// Number of times baggage was successfully written or updated on spans.
        /// </summary>
        ICounter BaggageUpdateSuccess { get; }

        /// <summary>
        /// Number of times baggage failed to write or update on spans.
        /// </summary>
        ICounter BaggageUpdateFailure { get; }

        /// <summary>
        /// Number of times baggage was truncated as per baggage restrictions.
        /// </summary>
        ICounter BaggageTruncate { get; }

        /// <summary>
        /// Number of times baggage restrictions were successfully updated.
        /// </summary>
        ICounter BaggageRestrictionsUpdateSuccess { get; }

        /// <summary>
        /// Number of times baggage restrictions failed to update.
        /// </summary>
        ICounter BaggageRestrictionsUpdateFailure { get; }
    }
}
