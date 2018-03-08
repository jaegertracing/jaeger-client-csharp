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
        private ILetsTraceTracer _tracer;
        private string _operationName;
        private bool _ignoreActiveSpan = false;
        private List<Reference> _references = new List<Reference>();
        private ISampler _sampler;
        private IMetrics _metrics;
        private DateTimeOffset? _startTimestamp;
        private Dictionary<string, Field> _tags = new Dictionary<string, Field>();

        public SpanBuilder(ILetsTraceTracer tracer, string operationName, ISampler sampler, IMetrics metrics)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            _references.Add(new Reference(referenceType, referencedContext));
            return this;
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            _ignoreActiveSpan = true;
            return this;
        }

        public ISpanBuilder AsChildOf(ISpan parent) => AsChildOf(parent.Context);

        public ISpanBuilder AsChildOf(ISpanContext parent) => AddReference(References.ChildOf, parent);

        public ISpanBuilder FollowsFrom(ISpan parent) => FollowsFrom(parent.Context);

        public ISpanBuilder FollowsFrom(ISpanContext parent) => AddReference(References.FollowsFrom, parent);

        public IScope StartActive(bool finishSpanOnDispose)
        {
            if (_tracer.ScopeManager.Active != null && _references.Count == 0 && !_ignoreActiveSpan)
            {
                AsChildOf(_tracer.ScopeManager.Active.Span.Context);
            }

            return _tracer.ScopeManager.Activate(Start(), finishSpanOnDispose);
        }
        
        public ISpan Start()
        {
            SpanContext parent = null;
            foreach(var reference in _references)
            {
                if (reference.Type == References.ChildOf) {
                    parent = reference.Context as SpanContext;
                    break;
                }
            }

            var spanContext = parent != null 
                ? CreateRootSpanContext(parent) 
                : CreateChildSpanContext();

            var span = new Span(_tracer, _operationName, spanContext, _startTimestamp, _tags, _references);
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
            var traceId = new TraceId
            {
                /*High = RandomGenerator.RandomId(),*/
                Low = RandomGenerator.RandomId()
            };
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
            _startTimestamp = startTimestamp;
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