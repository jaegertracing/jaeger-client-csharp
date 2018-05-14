using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Tag;

namespace Jaeger.Core
{
    public class Span : ISpan
    {
        private static readonly IReadOnlyList<LogData> EmptyLogs = new List<LogData>().AsReadOnly();
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

        public IReadOnlyDictionary<string, object> GetTags() => _tags;

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
                return _logs ?? EmptyLogs;
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
                    Tracer.Logger.LogWarning("Span has already been finished; will not be reported again.");
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

        public ISpan Log(IDictionary<string, object> fields)
        {
            return LogInternal(Tracer.Clock.UtcNow(), fields);
        }

        public ISpan Log(DateTimeOffset timestamp, IDictionary<string, object> fields)
        {
            return LogInternal(timestamp.UtcDateTime, fields);
        }

        private ISpan LogInternal(DateTime timestampUtc, IDictionary<string, object> fields)
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
        /// <param name="fields">Dictionary containing exception logs which are not present in fields.</param>
        /// <returns>Logged fields.</returns>
        private static IDictionary<string, object> AddExceptionLogs(IDictionary<string, object> fields)
        {
            if (!fields.TryGetValue(LogFields.ErrorObject, out object value) || !(value is Exception ex))
            {
                return fields;
            }

            var errorFields = new Dictionary<string, object>(fields);

            if (!fields.ContainsKey(LogFields.ErrorKind))
            {
                errorFields[LogFields.ErrorKind] = ex.GetType().FullName;
            }
            if (!fields.ContainsKey(LogFields.Message))
            {
                string message = ex.Message;
                if (message != null)
                {
                    errorFields[LogFields.Message] = message;
                }
            }
            if (!fields.ContainsKey(LogFields.Stack))
            {
                errorFields[LogFields.Stack] = ex.StackTrace;
            }
            return errorFields;
        }
    }
}