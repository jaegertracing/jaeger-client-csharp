using System;
using System.Collections.Generic;
using Jaeger.Core.Metrics;

namespace Jaeger.Core.Baggage
{
    /// <summary>
    /// <see cref="BaggageSetter"/> is a class that sets baggage and the logs associated
    /// with the baggage on a given <see cref="Span"/>.
    /// </summary>
    public sealed class BaggageSetter
    {
        private readonly IBaggageRestrictionManager _restrictionManager;
        private readonly IMetrics _metrics;

        public BaggageSetter(IBaggageRestrictionManager restrictionManager, IMetrics metrics)
        {
            _restrictionManager = restrictionManager ?? throw new ArgumentNullException(nameof(restrictionManager));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        /// <summary>
        /// Sets the baggage key:value on the <see cref="Span"/> and the corresponding
        /// logs. Whether the baggage is set on the span depends on if the key
        /// is allowed to be set by this service.
        /// <para/>
        /// A <see cref="SpanContext"/> is returned with the new baggage key:value set
        /// if key is valid, else returns the existing <see cref="SpanContext"/>
        /// on the <see cref="Span"/>.
        /// </summary>
        /// <param name="span">The span to set the baggage on.</param>
        /// <param name="key">The baggage key to set.</param>
        /// <param name="value">the baggage value to set.</param>
        /// <returns>The <see cref="SpanContext"/> with the baggage set.</returns>
        public SpanContext SetBaggage(Span span, string key, string value)
        {
            Restriction restriction = _restrictionManager.GetRestriction(span.Tracer.ServiceName, key);
            bool truncated = false;
            string prevItem = null;

            if (!restriction.KeyAllowed)
            {
                _metrics.BaggageUpdateFailure.Inc(1);
                LogFields(span, key, value, prevItem, truncated, restriction.KeyAllowed);
                return span.Context;
            }

            if (value != null && value.Length > restriction.MaxValueLength)
            {
                truncated = true;
                value = value.Substring(0, restriction.MaxValueLength);
                _metrics.BaggageTruncate.Inc(1);
            }

            prevItem = span.GetBaggageItem(key);
            LogFields(span, key, value, prevItem, truncated, restriction.KeyAllowed);
            _metrics.BaggageUpdateSuccess.Inc(1);

            return span.Context.WithBaggageItem(key, value);
        }

        private void LogFields(Span span, string key, string value, string prevItem, bool truncated, bool valid)
        {
            if (!span.Context.IsSampled)
            {
                return;
            }

            var fields = new Dictionary<string, object>();
            fields["event"] = "baggage";
            fields["key"] = key;
            fields["value"] = value;

            if (prevItem != null)
            {
                fields["override"] = true;
            }
            if (truncated)
            {
                fields["truncated"] = true;
            }
            if (!valid)
            {
                fields["invalid"] = true;
            }

            span.Log(fields);
        }
    }
}