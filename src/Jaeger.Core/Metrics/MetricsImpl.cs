using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jaeger.Core.Metrics
{
    public class MetricsImpl : IMetrics
    {
        /// <inheritdoc/>
        [Metric(name: "traces", tags: "state=started,sampled=y")]
        public ICounter TraceStartedSampled { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "traces", tags: "state=started,sampled=n")]
        public ICounter TraceStartedNotSampled { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "traces", tags: "state=joined,sampled=y")]

        public ICounter TracesJoinedSampled { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "traces", tags: "state=joined,sampled=n")]
        public ICounter TracesJoinedNotSampled { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "started_spans", tags: "sampled=y")]
        public ICounter SpansStartedSampled { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "started_spans", tags: "sampled=n")]
        public ICounter SpansStartedNotSampled { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "finished_spans")]
        public ICounter SpansFinished { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "span_context_decoding_errors")]
        public ICounter DecodingErrors { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "reporter_spans", tags: "result=ok")]
        public ICounter ReporterSuccess { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "reporter_spans", tags: "result=err")]
        public ICounter ReporterFailure { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "reporter_spans", tags: "result=dropped")]
        public ICounter ReporterDropped { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "reporter_queue_length")]
        public IGauge ReporterQueueLength { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "sampler_queries", tags: "result=ok")]
        public ICounter SamplerRetrieved { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "sampler_queries", tags: "result=err")]
        public ICounter SamplerQueryFailure { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "sampler_updates", tags: "result=ok")]
        public ICounter SamplerUpdated { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "sampler_updates", tags: "result=err")]
        public ICounter SamplerParsingFailure { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "baggage_updates", tags: "result=ok")]
        public ICounter BaggageUpdateSuccess { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "baggage_updates", tags: "result=err")]
        public ICounter BaggageUpdateFailure { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "baggage_truncations")]
        public ICounter BaggageTruncate { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "baggage_restrictions_updates", tags: "result=ok")]
        public ICounter BaggageRestrictionsUpdateSuccess { get; private set; }

        /// <inheritdoc/>
        [Metric(name: "baggage_restrictions_updates", tags: "result=err")]
        public ICounter BaggageRestrictionsUpdateFailure { get; private set; }

        public MetricsImpl(IMetricsFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            CreateMetrics(factory);
        }

        private void CreateMetrics(IMetricsFactory factory)
        {
            foreach (PropertyInfo property in typeof(MetricsImpl).GetProperties())
            {
                Type metricType = property.PropertyType;

                if (!typeof(ICounter).IsAssignableFrom(metricType)
                      && !typeof(ITimer).IsAssignableFrom(metricType)
                      && !typeof(IGauge).IsAssignableFrom(metricType))
                {
                    // Some frameworks dynamically add code that this reflection will pick up
                    // I only want this classes Stats based fields to be picked up.
                    continue;
                }

                StringBuilder metricBuilder = new StringBuilder("jaeger:");
                Dictionary<string, string> tags = new Dictionary<string, string>();

                var metricAttributes = property.GetCustomAttributes<MetricAttribute>();
                foreach (MetricAttribute metricAttribute in metricAttributes)
                {
                    metricBuilder.Append(metricAttribute.Name);
                    foreach (var tag in metricAttribute.Tags)
                    {
                        tags[tag.Key] = tag.Value;
                    }
                }

                string metricName = metricBuilder.ToString();

                if (metricType == typeof(ICounter))
                {
                    property.SetValue(this, factory.CreateCounter(metricName, tags));
                }
                else if (metricType == typeof(IGauge))
                {
                    property.SetValue(this, factory.CreateGauge(metricName, tags));
                }
                else if (metricType == typeof(ITimer))
                {
                    property.SetValue(this, factory.CreateTimer(metricName, tags));
                }
                else
                {
                    throw new NotSupportedException($"'{metricType}' for metric '{property.Name}' is not supported.");
                }
            }
        }

        public static string AddTagsToMetricName(string name, Dictionary<string, string> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return name;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(name);

            SortedDictionary<string, string> sortedTags = new SortedDictionary<string, string>(tags);
            foreach (var entry in sortedTags)
            {
                sb.Append(".");
                sb.Append(entry.Key);
                sb.Append("=");
                sb.Append(entry.Value);
            }

            return sb.ToString();
        }
    }
}