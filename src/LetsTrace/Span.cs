using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing;

namespace LetsTrace
{
    // Span holds everything relevant to a specific span
    public class Span : ILetsTraceSpan
    {
        // OpenTracing API: Retrieve the Spans SpanContext
        // C# doesn't have "return type covariance" so we use the trick with the explicit interface implementation
        // and this separate property.
        public ILetsTraceSpanContext Context { get; }
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
        public Dictionary<string, Field> Tags { get; }
        public ILetsTraceTracer Tracer { get; }

        public Span(ILetsTraceTracer tracer, string operationName, ILetsTraceSpanContext context, DateTime? startTimestampUtc = null, Dictionary<string, Field> tags = null, List<Reference> references = null)
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
            Tags = tags ?? new Dictionary<string, Field>();
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
            return LogInternal(Tracer.Clock.UtcNow(), fields.ToFieldList());
        }

        // OpenTracing API: Log structured data
        public ISpan Log(DateTimeOffset timestamp, IDictionary<string, object> fields)
        {
            return LogInternal(timestamp.UtcDateTime, fields.ToFieldList());
        }

        // OpenTracing API: Log structured data
        public ISpan Log(string eventName)
        {
            return LogInternal(Tracer.Clock.UtcNow(), new List<Field> { new Field<string> { Key = LogFields.Event, Value = eventName } });
        }

        // OpenTracing API: Log structured data
        public ISpan Log(DateTimeOffset timestamp, string eventName)
        {
            return LogInternal(timestamp.UtcDateTime, new List<Field> { new Field<string> { Key = LogFields.Event, Value = eventName } });
        }

        private ISpan LogInternal(DateTime timestampUtc, List<Field> fields)
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
        public ISpan SetTag(string key, bool value) => SetTag(key, new Field<bool> { Key = key, Value = value });

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, double value) => SetTag(key, new Field<double> { Key = key, Value = value });

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, int value) => SetTag(key, new Field<int> { Key = key, Value = value });

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, string value) => SetTag(key, new Field<string> { Key = key, Value = value });

        private ISpan SetTag(string key, Field value)
        {
            Tags[key] = value;
            return this;
        }
    }
}