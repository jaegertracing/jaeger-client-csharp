using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Tag;

namespace Jaeger
{
    public class Span : ISpan
    {
        private static readonly IReadOnlyList<LogData> EmptyLogs = new List<LogData>().AsReadOnly();
        private static readonly IReadOnlyDictionary<string, object> EmptyTags = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
        private static readonly IReadOnlyList<Reference> EmptyReferences = new List<Reference>().AsReadOnly();

        private readonly object _lock = new object();

        private readonly Dictionary<string, object> _tags;
        private readonly IReadOnlyList<Reference> _references;

        // We don't allocate if there's no logs.
        private List<LogData> _logs;

        public Tracer Tracer { get; }
        public DateTime StartTimestampUtc { get; }
        public string OperationName { get; private set; }

        // C# doesn't have "return type covariance" so we use the trick with the explicit interface implementation
        // and this separate property.
        public SpanContext Context { get; private set; }
        ISpanContext ISpan.Context => Context;

        public DateTime? FinishTimestampUtc { get; private set; }

        internal Span(
            Tracer tracer,
            string operationName,
            SpanContext context,
            DateTime startTimestampUtc,
            Dictionary<string, object> tags,
            IReadOnlyList<Reference> references)
        {
            Tracer = tracer;
            OperationName = operationName;
            Context = context;
            StartTimestampUtc = startTimestampUtc;
            _references = references ?? EmptyReferences;

            _tags = new Dictionary<string, object>();
            foreach (var tag in tags)
            {
                SetTagAsObject(tag.Key, tag.Value);
            }
        }

        public IReadOnlyList<Reference> GetReferences() => _references;

        public IReadOnlyDictionary<string, object> GetTags()
        {
            lock (_lock)
            {
                if (_tags == null)
                {
                    return EmptyTags;
                }

                return new Dictionary<string, object>(_tags);
            }
        }

        public ISpan SetOperationName(string operationName)
        {
            lock (_lock)
            {
                OperationName = operationName;
            }
            return this;
        }

        public string GetServiceName()
        {
            return Tracer.ServiceName;
        }

        public IReadOnlyList<LogData> GetLogs()
        {
            lock (_lock)
            {
                if (_logs == null)
                {
                    return EmptyLogs;
                }

                return new List<LogData>(_logs);
            }
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            if (key == null || (value == null && Context.GetBaggageItem(key) == null))
            {
                //Ignore attempts to add new baggage items with null values, they're not accessible anyway.
                return this;
            }

            lock (_lock)
            {
                Context = Tracer.SetBaggage(this, key, value);
            }
            return this;
        }

        public string GetBaggageItem(string key)
        {
            lock (_lock)
            {
                return Context.GetBaggageItem(key);
            }
        }

        public override string ToString()
        {
            lock (_lock)
            {
                return Context.ContextAsString() + " - " + OperationName;
            }
        }

        public void Finish() => FinishInternal(Tracer.Clock.UtcNow());

        public void Finish(DateTimeOffset finishTimestamp) => FinishInternal(finishTimestamp.UtcDateTime);

        private void FinishInternal(DateTime finishTimestampUtc)
        {
            lock (_lock)
            {
                if (FinishTimestampUtc != null)
                {
                    Tracer.Logger.LogWarning("Span has already been finished; will not be reported again. Operation: {operationName} Trace Id: {traceId} Span Id: {spanId}", OperationName, Context.TraceId, Context.SpanId);
                    return;
                }
                FinishTimestampUtc = finishTimestampUtc;
            }

            if (Context.IsSampled)
            {
                Tracer.ReportSpan(this);
            }
        }

        public ISpan SetTag(string key, bool value) => SetTagAsObject(key, value);

        public ISpan SetTag(string key, double value) => SetTagAsObject(key, value);

        public ISpan SetTag(string key, int value) => SetTagAsObject(key, value);

        public ISpan SetTag(string key, string value) => SetTagAsObject(key, value);

        public ISpan SetTag(BooleanTag tag, bool value) => SetTagAsObject(tag.Key, value);

        public ISpan SetTag(IntOrStringTag tag, string value) => SetTagAsObject(tag.Key, value);

        public ISpan SetTag(IntTag tag, int value) => SetTagAsObject(tag.Key, value);

        public ISpan SetTag(StringTag tag, string value) => SetTagAsObject(tag.Key, value);

        private ISpan SetTagAsObject(string key, object value)
        {
            lock (_lock)
            {
                if (string.Equals(key, Tags.SamplingPriority.Key, StringComparison.Ordinal) && (value is int priority))
                {
                    SpanContextFlags newFlags;
                    if (priority > 0)
                    {
                        newFlags = Context.Flags | SpanContextFlags.Sampled | SpanContextFlags.Debug;
                    }
                    else
                    {
                        newFlags = Context.Flags & ~SpanContextFlags.Sampled;
                    }

                    Context = Context.WithFlags(newFlags);
                }

                if (Context.IsSampled)
                {
                    _tags[key] = value;
                }
            }
            return this;
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return LogInternal(Tracer.Clock.UtcNow(), fields);
        }

        public ISpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            return LogInternal(timestamp.UtcDateTime, fields);
        }

        private ISpan LogInternal(DateTime timestampUtc, IEnumerable<KeyValuePair<string, object>> fields)
        {
            if (fields == null)
                return this;

            lock (_lock)
            {
                if (Context.IsSampled)
                {
                    if (Tracer.ExpandExceptionLogs)
                    {
                        fields = AddExceptionLogs(fields);
                    }
                    if (_logs == null)
                    {
                        _logs = new List<LogData>();
                    }
                    _logs.Add(new LogData(timestampUtc, fields));
                }
            }
            return this;
        }

        public ISpan Log(string @event)
        {
            return LogInternal(Tracer.Clock.UtcNow(), @event);
        }

        public ISpan Log(DateTimeOffset timestamp, string @event)
        {
            return LogInternal(timestamp.UtcDateTime, @event);
        }

        private ISpan LogInternal(DateTime timestampUtc, string @event)
        {
            if (string.IsNullOrEmpty(@event))
                return this;

            lock (_lock)
            {
                if (Context.IsSampled)
                {
                    if (_logs == null)
                    {
                        _logs = new List<LogData>();
                    }
                    _logs.Add(new LogData(timestampUtc, @event));
                }
            }
            return this;
        }

        /// <summary>
        /// Creates logs related to logged exception.
        /// </summary>
        /// <param name="fields">Current logging fields.</param>
        /// <returns>Logged fields.</returns>
        private static IEnumerable<KeyValuePair<string, object>> AddExceptionLogs(IEnumerable<KeyValuePair<string, object>> fields)
        {
            var errorFields = new Dictionary<string, object>();
            foreach (var kvp in fields)
            {
                errorFields[kvp.Key] = kvp.Value;
            }

            if (!errorFields.TryGetValue(LogFields.ErrorObject, out object value) || !(value is Exception ex))
            {
                return fields;
            }

            if (!errorFields.ContainsKey(LogFields.ErrorKind))
            {
                errorFields[LogFields.ErrorKind] = ex.GetType().FullName;
            }
            if (!errorFields.ContainsKey(LogFields.Message))
            {
                string message = ex.Message;
                if (message != null)
                {
                    errorFields[LogFields.Message] = message;
                }
            }
            if (!errorFields.ContainsKey(LogFields.Stack))
            {
                errorFields[LogFields.Stack] = ex.StackTrace;
            }
            return errorFields;
        }
    }
}
