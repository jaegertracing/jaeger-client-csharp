using System;
using System.Collections.Generic;
using System.Linq;
using LetsTrace.Util;
using OpenTracing;

namespace LetsTrace
{
    public class SpanBuilder : ISpanBuilder
    {
        private ILetsTraceTracer _tracer;
        private string _operationName;
        private List<Reference> _references = new List<Reference>();
        private DateTimeOffset? _startTimestamp;
        private Dictionary<string, string> _tags = new Dictionary<string, string>();

        public SpanBuilder(ILetsTraceTracer tracer, string operationName)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            _references.Add(new Reference(referenceType, referencedContext));
            return this;
        }

        public ISpanBuilder AsChildOf(ISpan parent) => AsChildOf(parent.Context);

        public ISpanBuilder AsChildOf(ISpanContext parent) => AddReference(References.ChildOf, parent);

        public ISpanBuilder FollowsFrom(ISpan parent) => FollowsFrom(parent.Context);

        public ISpanBuilder FollowsFrom(ISpanContext parent) => AddReference(References.FollowsFrom, parent);

        public ISpan Start()
        {
            SpanContext parent = null;
            TraceId traceId = null;
            SpanId parentId = null;
            SpanId spanId = new SpanId(RandomGenerator.RandomId());
            Dictionary<string, string> baggage = null;

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
                baggage = parent.GetBaggageItems().ToDictionary(x => x.Key, x => x.Value);
            } 
            else
            {
                traceId = new TraceId{ High = RandomGenerator.RandomId(), Low = RandomGenerator.RandomId() };
                parentId = new SpanId(0);
            }

            var spanContext = new SpanContext(traceId, spanId, parentId, baggage);

            return new Span(_tracer, _operationName, spanContext, _startTimestamp, _tags, _references);
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset startTimestamp)
        {
            _startTimestamp = startTimestamp;
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value) => WithTag(key, value.ToString());

        public ISpanBuilder WithTag(string key, double value) => WithTag(key, value.ToString());

        public ISpanBuilder WithTag(string key, int value) => WithTag(key, value.ToString());

        public ISpanBuilder WithTag(string key, string value)
        {
            _tags.Add(key, value);
            return this;
        }
    }
}