using System;
using System.Collections.Generic;
using System.Linq;
using LetsTrace.Metrics;
using LetsTrace.Samplers;
using LetsTrace.Util;
using OpenTracing;

namespace LetsTrace
{
    public class SpanBuilder : ISpanBuilder
    {
        private readonly ILetsTraceTracer _tracer;
        private readonly string _operationName;
        private readonly ISampler _sampler;
        private readonly IMetrics _metrics;
        private readonly Dictionary<string, Field> _tags;

        private List<Reference> _references;
        private bool _ignoreActiveSpan;
        private DateTime? _startTimestampUtc;

        public SpanBuilder(ILetsTraceTracer tracer, string operationName, ISampler sampler, IMetrics metrics)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));

            // There will be tags in most cases so it should be fine to always initiate this variable.
            _tags = new Dictionary<string, Field>();
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (referencedContext != null)
            {
                if (_references == null)
                {
                    // Adding an item to a new list normally creates an array with 4 items.
                    // Since we'll either have zero or one reference in 99% of all cases, we can optimize this
                    // and provide better performance for zero/one references with slightly worse perf for multiple references.
                    _references = new List<Reference>(1);
                }

                _references.Add(new Reference(referenceType, referencedContext));
            }
            return this;
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            _ignoreActiveSpan = true;
            return this;
        }

        public ISpanBuilder AsChildOf(ISpan parent) => AsChildOf(parent?.Context);

        public ISpanBuilder AsChildOf(ISpanContext parent) => AddReference(References.ChildOf, parent);

        public IScope StartActive(bool finishSpanOnDispose)
        {
            return _tracer.ScopeManager.Activate(Start(), finishSpanOnDispose);
        }

        public ISpan Start()
        {
            if (!_ignoreActiveSpan && _references == null)
            {
                AsChildOf(_tracer.ActiveSpan);
            }

            SpanContext parent = null;
            if (_references != null)
            {
                foreach (var reference in _references)
                {
                    if (reference.Type == References.ChildOf)
                    {
                        parent = reference.Context as SpanContext;
                        break;
                    }
                }
            }

            var spanContext = parent != null
                ? CreateRootSpanContext(parent)
                : CreateChildSpanContext();

            var span = new Span(_tracer, _operationName, spanContext, _startTimestampUtc, _tags, _references);
            if (spanContext.IsSampled)
            {
                _metrics.SpansStartedSampled.Inc(1);
            }
            else
            {
                _metrics.SpansStartedNotSampled.Inc(1);
            }
            return span;
        }

        private SpanContext CreateRootSpanContext(SpanContext parent)
        {
            var traceId = parent.TraceId;
            var parentId = parent.SpanId;
            var spanId = new SpanId(RandomGenerator.RandomId());
            var baggage = parent.GetBaggageItems().ToDictionary(x => x.Key, x => x.Value);
            var flags = parent.Flags;

            if (parent.IsSampled)
            {
                _metrics.TracesJoinedSampled.Inc(1);
            }
            else
            {
                _metrics.TracesJoinedNotSampled.Inc(1);
            }

            var spanContext = new SpanContext(traceId, spanId, parentId, baggage, flags);
            return spanContext;
        }

        private SpanContext CreateChildSpanContext()
        {
            var traceId = new TraceId(RandomGenerator.RandomId(), RandomGenerator.RandomId());
            var parentId = new SpanId(0);
            var spanId = new SpanId(traceId.Low);
            var baggage = new Dictionary<string, string>();
            var flags = ContextFlags.None;

            var (isSampled, samplerTags) = _sampler.IsSampled(traceId, _operationName);
            if (isSampled)
            {
                foreach (var samplingTag in samplerTags)
                {
                    _tags[samplingTag.Key] = samplingTag.Value;
                }

                flags |= ContextFlags.Sampled;
                _metrics.TraceStartedSampled.Inc(1);
            }
            else
            {
                _metrics.TraceStartedNotSampled.Inc(1);
            }

            var spanContext = new SpanContext(traceId, spanId, parentId, baggage, flags);
            return spanContext;
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset startTimestamp)
        {
            _startTimestampUtc = startTimestamp.UtcDateTime;
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value) => WithTag(key, new Field<bool> { Key = key, Value = value });

        public ISpanBuilder WithTag(string key, double value) => WithTag(key, new Field<double> { Key = key, Value = value });

        public ISpanBuilder WithTag(string key, int value) => WithTag(key, new Field<int> { Key = key, Value = value });

        public ISpanBuilder WithTag(string key, string value) => WithTag(key, new Field<string> { Key = key, Value = value });

        private ISpanBuilder WithTag(string key, Field value)
        {
            _tags[key] = value;
            return this;
        }
    }
}