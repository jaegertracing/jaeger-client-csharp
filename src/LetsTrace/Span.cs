using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing;

namespace LetsTrace
{
    // Span holds everything relevant to a specific span
    public class Span : ILetsTraceSpan
    {
        // private members
        private ILetsTraceTracer _tracer { get; }


        // public members
        
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
        public Dictionary<string, string> Tags { get; private set; }

        public Span(ILetsTraceTracer tracer, string operationName, ISpanContext context, DateTimeOffset? startTimestamp = null, Dictionary<string, string> tags = null, List<Reference> references = null)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));

            if (string.IsNullOrEmpty(operationName))
            {
                var nullOrEmpty = operationName == null ? "null" : "empty";
                throw new ArgumentException($"Argument is {nullOrEmpty}", nameof(operationName));
            }

            OperationName = operationName;
            Context = context ?? throw new ArgumentNullException(nameof(context));

            StartTimestamp = startTimestamp ?? _tracer.Clock.CurrentTime();
            Tags = tags ?? new Dictionary<string, string>();
            References = references ?? new List<Reference>();
        }

        public void Dispose() => Finish();

        // OpenTracing API: Finish the Span
        public void Finish() => Finish(_tracer.Clock.CurrentTime());

        // OpenTracing API: Finish the Span
        // An explicit finish timestamp for the Span
        public void Finish(DateTimeOffset finishTimestamp)
        {
            if (FinishTimestamp == null) // only report if it's not finished yet
            {
                FinishTimestamp = finishTimestamp;
                _tracer.ReportSpan(this);
            }
        }

        // OpenTracing API: Get a baggage item
        public string GetBaggageItem(string key) => Context.GetBaggageItems().Where(bi => bi.Key == key).Select(bi => bi.Value).FirstOrDefault();

        // OpenTracing API: Set a baggage item
        public ISpan SetBaggageItem(string key, string value) => _tracer.SetBaggageItem(this, key, value);

        // OpenTracing API: Log structured data
        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields) => Log(_tracer.Clock.CurrentTime(), fields);

        // OpenTracing API: Log structured data
        public ISpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields) => Log(timestamp, null, fields);

        // OpenTracing API: Log structured data
        public ISpan Log(string eventName) => Log(_tracer.Clock.CurrentTime(), eventName, null);

        // OpenTracing API: Log structured data
        public ISpan Log(DateTimeOffset timestamp, string eventName) => Log(timestamp, eventName, null);

        private ISpan Log(DateTimeOffset timestamp, string message, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Logs.Add(new LogRecord(timestamp, message, fields));
            return this;
        }

        //  OpenTracing API: Overwrite the operation name
        public ISpan SetOperationName(string operationName)
        {
            OperationName = operationName;
            return this;
        }

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, bool value) =>  SetTag(key, value.ToString());

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, double value) =>  SetTag(key, value.ToString());

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, int value) =>  SetTag(key, value.ToString());

        // OpenTracing API: Set a Span tag
        public ISpan SetTag(string key, string value)
        {
            Tags[key] = value;
            return this;
        }
    }
}