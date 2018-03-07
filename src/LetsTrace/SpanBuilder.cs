using System;
using System.Collections.Generic;
using System.Linq;
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
        private DateTimeOffset? _startTimestamp;
        private Dictionary<string, Field> _tags = new Dictionary<string, Field>();

        public SpanBuilder(ILetsTraceTracer tracer, string operationName, ISampler sampler)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
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
            TraceId traceId = null;
            SpanId parentId = null;
            SpanId spanId = null;
            Dictionary<string, string> baggage = null;
            ContextFlags flags = ContextFlags.None;

            foreach(var reference in _references)
            {
                if (reference.Type == References.ChildOf) {
                    parent = reference.Context as SpanContext;
                    break;
                }
            }

            if (parent != null)
            {
                traceId = parent.TraceId;
                parentId = parent.SpanId;
                spanId = new SpanId(RandomGenerator.RandomId());
                baggage = parent.GetBaggageItems().ToDictionary(x => x.Key, x => x.Value);
                flags = parent.Flags;
            } 
            else
            {
                traceId = new TraceId{ /*High = RandomGenerator.RandomId(),*/ Low = RandomGenerator.RandomId() }; // TODO: Jaeger does not support high values, otherwise root span id can not match
                parentId = new SpanId(0);
                spanId = new SpanId(traceId.Low);   // TODO: Jaeger expects the root span id to match the trace id, otherwise the operation name is not shown.
                var samplingInfo = _sampler.IsSampled(traceId, _operationName);
                foreach(var samplingTag in samplingInfo.Tags) {
                    _tags[samplingTag.Key] = samplingTag.Value;
                }
                flags = samplingInfo.Sampled ? ContextFlags.Sampled : ContextFlags.None;
            }

            var spanContext = new SpanContext(traceId, spanId, parentId, baggage, flags);

            return new Span(_tracer, _operationName, spanContext, _startTimestamp, _tags, _references);
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