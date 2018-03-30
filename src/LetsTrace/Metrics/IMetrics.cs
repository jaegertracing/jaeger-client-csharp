namespace Jaeger.Core.Metrics
{
    public interface IMetrics
    {
        // Number of traces started by this tracer as sampled
        [Metric("traces", state: MetricAttribute.MetricState.Started, sampled: MetricAttribute.MetricSampled.Yes)]
        ICounter TraceStartedSampled { get; }

        // Number of traces started by this tracer as not sampled
        [Metric("traces", state: MetricAttribute.MetricState.Started, sampled: MetricAttribute.MetricSampled.No)]
        ICounter TraceStartedNotSampled { get; }

        // Number of externally started sampled traces this tracer joined
        [Metric("traces", state: MetricAttribute.MetricState.Joined, sampled: MetricAttribute.MetricSampled.Yes)]
        ICounter TracesJoinedSampled { get; }

        // Number of externally started not-sampled traces this tracer joined
        [Metric("traces", state: MetricAttribute.MetricState.Joined, sampled: MetricAttribute.MetricSampled.No)]
        ICounter TracesJoinedNotSampled { get; }

        // Number of sampled spans started by this tracer
        [Metric("started_spans", sampled: MetricAttribute.MetricSampled.Yes)]
        ICounter SpansStartedSampled { get; }

        // Number of unsampled spans started by this tracer
        [Metric("started_spans", sampled: MetricAttribute.MetricSampled.No)]
        ICounter SpansStartedNotSampled { get; }

        // Number of spans finished by this tracer
        [Metric("finished_spans")]
        ICounter SpansFinished { get; }

        // Number of errors decoding tracing context
        [Metric("span_context_decoding_errors")]
        ICounter DecodingErrors { get; } // TODO: Not implemented yet!

        // Number of spans successfully reported
        [Metric("reporter_spans", result: MetricAttribute.MetricResult.Ok)]
        ICounter ReporterSuccess { get; }

        // Number of spans not reported due to a Sender failure
        [Metric("reporter_spans", result: MetricAttribute.MetricResult.Error)]
        ICounter ReporterFailure { get; }

        // Number of spans dropped due to internal queue overflow
        [Metric("reporter_spans", result: MetricAttribute.MetricResult.Dropped)]
        ICounter ReporterDropped { get; }

        // Current number of spans in the reporter queue
        [Metric("reporter_queue_length")]
        IGauge ReporterQueueLength { get; } // TODO: Not implemented yet!

        // Number of times the Sampler succeeded to retrieve sampling strategy
        [Metric("sampler_queries", result: MetricAttribute.MetricResult.Ok)]
        ICounter SamplerRetrieved { get; }

        // Number of times the Sampler failed to retrieve sampling strategy
        [Metric("sampler_queries", result: MetricAttribute.MetricResult.Error)]
        ICounter SamplerQueryFailure { get; }

        // Number of times the Sampler succeeded to retrieve and update sampling strategy
        [Metric("sampler_updates", result: MetricAttribute.MetricResult.Ok)]
        ICounter SamplerUpdated { get; }

        // Number of times the Sampler failed to update sampling strategy
        [Metric("sampler_updates", result: MetricAttribute.MetricResult.Error)]
        ICounter SamplerParsingFailure { get; }

        // Number of times baggage was successfully written or updated on spans.
        [Metric("baggage_updates", result: MetricAttribute.MetricResult.Ok)]
        ICounter BaggageUpdateSuccess { get; } // TODO: Not implemented yet!

        // Number of times baggage failed to write or update on spans
        [Metric("baggage_updates", result: MetricAttribute.MetricResult.Error)]
        ICounter BaggageUpdateFailure { get; } // TODO: Not implemented yet!

        // Number of times baggage was truncated as per baggage restrictions
        [Metric("baggage_truncations")]
        ICounter BaggageTruncate { get; } // TODO: Not implemented yet!

        // Number of times baggage restrictions were successfully updated.
        [Metric("baggage_restrictions_updates", result: MetricAttribute.MetricResult.Ok)]
        ICounter BaggageRestrictionsUpdateSuccess { get; } // TODO: Not implemented yet!

        // Number of times baggage restrictions failed to update.
        [Metric("baggage_restrictions_updates", result: MetricAttribute.MetricResult.Error)]
        ICounter BaggageRestrictionsUpdateFailure { get; } // TODO: Not implemented yet!
    }
}
