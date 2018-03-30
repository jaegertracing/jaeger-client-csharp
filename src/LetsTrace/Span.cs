using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing;

namespace Jaeger.Core
{
    // Span holds everything relevant to a specific span
    public class Span : IJaegerCoreSpan
    {
        // OpenTracing API: Retrieve the Spans SpanContext
        // C# doesn't have "return type covariance" so we use the trick with the explicit interface implementation
        // and this separate property.
        public IJaegerCoreSpanContext Context { get; }
        ISpanContext ISpan.Context => Context;

        public DateTime? FinishTimestampUtc { get; private set; }
        public List<LogRecord> Logs { get; } = new List<LogRecord>();
        public string OperationName { get; private set; }

        // these references are only references - when the span is built the
        // context that is given to it needs to already have determined who the
        // parent is (if there is one). The span should not handle determining
        // who its parent is
        public IEnumerable<Reference> References { get; }
        public DateTime StartTimestampUtc { get; }
        public Dictionary<string, object> Tags { get; }
        public IJaegerCoreTracer Tracer { get; }

        public Span(IJaegerCoreTracer tracer, string operationName, IJaegerCoreSpanContext context, DateTime? startTimestampUtc = null, Dictionary<string, object> tags = null, List<Reference> references = null)
        {
            Tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));

            if (string.IsNullOrEmpty(operationName))
            {
                var nullOrEmpty = operationName == null ? "null" : "empty";
                throw new ArgumentException($"Argument is {nullOrEmpty}", nameof(operationName));
            }

            OperationName = operationName;
            Context = context ?? throw new ArgumentNullException(nameof(context));
            StartTimestampUtc = startTimestampUtc ?? Tracer.Clock.UtcNow();
            Tags = tags ?? new Dictionary<string, object>();
            References = references ?? Enumerable.Empty<Reference>();
        }

        public void Dispose() => FinishInternal(Tracer.Clock.UtcNow());

        // OpenTracing API: Finish the Span
        public void Finish() => FinishInternal(Tracer.Clock.UtcNow());

        // OpenTracing API: Finish the Span
        // An explicit finish timestamp for the Span
        public void Finish(DateTimeOffset finishTimestamp) => FinishInternal(finishTimestamp.UtcDateTime);

        private void FinishInternal(DateTime finishTimestampUtc)
        {
            if (FinishTimestampUtc == null) // only report if it's not finished yet
            {
                FinishTimestampUtc = finishTimestampUtc;
                Tracer.ReportSpan(this);
            }
        }

        // OpenTracing API: Get a baggage item
        public string GetBaggageItem(string key) => Context.GetBaggageItems().Where(bi => bi.Key == key).Select(bi => bi.Value).FirstOrDefault();

        // OpenTracing API: Set a baggage item
        public ISpan SetBaggageItem(string key, string value) => Tracer.SetBaggageItem(this, key, value);

        // OpenTracing API: Log structured data
        public ISpan Log(IDictionary<string, object> fields)
        {
            return LogInternal(Tracer.Clock.UtcNow(), fields);
        }

        // OpenTracing API: Log structured data
        public ISpan Log(DateTimeOffset timestamp, IDictionary<string, object> fields)
        {
            return LogInternal(timestamp.UtcDateTime, fields);
        }

        // OpenTracing API: Log structured data
        public ISpan Log(string eventName)
        {
            return LogInternal(Tracer.Clock.UtcNow(), new Dictionary<string, object> { { LogFields.Event, eventName } });
        }

        // OpenTracing API: Log structured data
        public ISpan Log(DateTimeOffset timestamp, string eventName)
        {
            return LogInternal(timestamp.UtcDateTime, new Dictionary<string, object> { { LogFields.Event, eventName } });
        }

        private ISpan LogInternal(DateTime timestampUtc, IDictionary<string, object> fields)
        {
            Logs.Add(new LogRecord(timestampUtc, fields));
            return this;
        }

        //  OpenTracing API: Overwrite the operation name
        public ISpan SetOperationName(string operationName)
        {
            OperationName = operationName;
            return this;
        }

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, bool value) => SetTagAsObject(key, value);

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, double value) => SetTagAsObject(key, value);

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, int value) => SetTagAsObject(key, value);

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, string value) => SetTagAsObject(key, value);

        private ISpan SetTagAsObject(string key, object value)
        {
            Tags[key] = value;
            return this;
        }
    }
}