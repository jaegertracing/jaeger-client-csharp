using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing;
using OpenTracing.Tag;

namespace LetsTrace
{
    // Span holds everything relevant to a specific span
    public class Span : ILetsTraceSpan
    {
        // OpenTracing API: Retrieve the Spans SpanContext
        public ISpanContext Context { get; private set; }
        public DateTimeOffset? FinishTimestamp { get; private set; }
        public List<LogRecord> Logs { get; private set; } = new List<LogRecord>();
        public string OperationName { get; private set; }

        // these references are only references - when the span is built the
        // context that is given to it needs to already have determined who the
        // parent is (if there is one). The span should not handle determining
        // who its parent is
        public List<Reference> References { get; }
        public DateTimeOffset StartTimestamp { get; private set; }
        public Dictionary<string, Field> Tags { get; private set; }
        public ILetsTraceTracer Tracer { get; }

        public Span(ILetsTraceTracer tracer, string operationName, ISpanContext context, DateTimeOffset? startTimestamp = null, Dictionary<string, Field> tags = null, List<Reference> references = null)
        {
            Tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));

            if (string.IsNullOrEmpty(operationName))
            {
                var nullOrEmpty = operationName == null ? "null" : "empty";
                throw new ArgumentException($"Argument is {nullOrEmpty}", nameof(operationName));
            }

            OperationName = operationName;
            Context = context ?? throw new ArgumentNullException(nameof(context));

            StartTimestamp = startTimestamp ?? Tracer.Clock.CurrentTime();
            Tags = tags ?? new Dictionary<string, Field>();
            References = references ?? new List<Reference>();
        }

        public void Dispose() => Finish();

        // OpenTracing API: Finish the Span
        public void Finish() => Finish(Tracer.Clock.CurrentTime());

        // OpenTracing API: Finish the Span
        // An explicit finish timestamp for the Span
        public void Finish(DateTimeOffset finishTimestamp)
        {
            if (FinishTimestamp == null) // only report if it's not finished yet
            {
                FinishTimestamp = finishTimestamp;
                Tracer.ReportSpan(this);
            }
        }

        // OpenTracing API: Get a baggage item
        public string GetBaggageItem(string key) => Context.GetBaggageItems().Where(bi => bi.Key == key).Select(bi => bi.Value).FirstOrDefault();

        // OpenTracing API: Set a baggage item
        public ISpan SetBaggageItem(string key, string value) => Tracer.SetBaggageItem(this, key, value);

        // OpenTracing API: Log structured data
        public ISpan Log(IDictionary<string, object> fields) => Log(Tracer.Clock.CurrentTime(), fields);

        // OpenTracing API: Log structured data
        public ISpan Log(DateTimeOffset timestamp, IDictionary<string, object> fields) => Log(timestamp, fields.ToFieldList());

        // OpenTracing API: Log structured data
        public ISpan Log(string eventName) => Log(Tracer.Clock.CurrentTime(), eventName);

        // OpenTracing API: Log structured data
        public ISpan Log(DateTimeOffset timestamp, string eventName) => Log(timestamp, new List<Field> { new Field<string> { Key = "event", Value = eventName } });

        private ISpan Log(DateTimeOffset timestamp, IEnumerable<Field> fields)
        {
            Logs.Add(new LogRecord(timestamp, fields.ToList()));
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