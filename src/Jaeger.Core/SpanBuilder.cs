using System;
using System.Collections.Generic;
using OpenTracing;
using OpenTracing.Tag;

namespace Jaeger.Core
{
    internal class SpanBuilder : ISpanBuilder
    {
        private readonly Tracer _tracer;
        private readonly string _operationName;
        private readonly Dictionary<string, object> _tags;

        private DateTime? _startTimestampUtc;
        private List<Reference> _references;
        private bool _ignoreActiveSpan;

        internal SpanBuilder(Tracer tracer, string operationName)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));

            // There will be tags in most cases so it should be fine to always initiate this variable.
            _tags = new Dictionary<string, object>();
        }

        public ISpanBuilder AsChildOf(ISpan parent) => AsChildOf(parent?.Context);

        public ISpanBuilder AsChildOf(ISpanContext parent) => AddReference(References.ChildOf, parent);

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (!(referencedContext is SpanContext typedReferencedContext))
                return this;

            // Jaeger thrift currently does not support other reference types
            if (!string.Equals(References.ChildOf, referenceType, StringComparison.Ordinal)
                && !string.Equals(References.FollowsFrom, referenceType, StringComparison.Ordinal))
            {
                return this;
            }

            if (_references == null)
            {
                // Adding an item to a new list normally creates an array with 4 items.
                // Since we'll either have zero or one reference in 99% of all cases, we can optimize this
                // and provide better performance for zero/one references with slightly worse perf for multiple references.
                _references = new List<Reference>(1);
            }

            _references.Add(new Reference(typedReferencedContext, referenceType));
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            _tags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            _tags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            _tags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            _tags[key] = value;
            return this;
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset startTimestamp)
        {
            _startTimestampUtc = startTimestamp.UtcDateTime;
            return this;
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            _ignoreActiveSpan = true;
            return this;
        }

        public IScope StartActive(bool finishSpanOnDispose)
        {
            return _tracer.ScopeManager.Activate(Start(), finishSpanOnDispose);
        }

        public ISpan Start()
        {
            // Check if active span should be established as ChildOf relationship
            if (!_ignoreActiveSpan && _references == null)
            {
                AsChildOf(_tracer.ActiveSpan);
            }

            SpanContext context = null;
            string debugId = DebugId();
            if (_references == null)
            {
                context = CreateNewContext(null);
            }
            else if (debugId != null)
            {
                context = CreateNewContext(debugId);
            }
            else
            {
                context = CreateChildContext();
            }

            if (_startTimestampUtc == null)
            {
                _startTimestampUtc = _tracer.Clock.UtcNow();
            }

            var span = new Span(_tracer, _operationName, context, _startTimestampUtc.Value, _tags, _references);
            if (context.IsSampled)
            {
                _tracer.Metrics.SpansStartedSampled.Inc(1);
            }
            else
            {
                _tracer.Metrics.SpansStartedNotSampled.Inc(1);
            }
            return span;
        }

        private SpanContext CreateNewContext(string debugId)
        {
            TraceId traceId = TraceId.NewUniqueId();
            SpanId spanId = new SpanId(traceId);

            var flags = SpanContextFlags.None;
            if (debugId != null)
            {
                flags |= SpanContextFlags.Sampled | SpanContextFlags.Debug;
                _tags[Constants.DebugIdHeaderKey] = debugId;
                _tracer.Metrics.TraceStartedSampled.Inc(1);
            }
            else
            {
                var samplingStatus = _tracer.Sampler.Sample(_operationName, traceId);
                if (samplingStatus.IsSampled)
                {
                    flags |= SpanContextFlags.Sampled;
                    foreach (var samplingTag in samplingStatus.Tags)
                    {
                        _tags[samplingTag.Key] = samplingTag.Value;
                    }
                    _tracer.Metrics.TraceStartedSampled.Inc(1);
                }
                else
                {
                    _tracer.Metrics.TraceStartedNotSampled.Inc(1);
                }
            }

            return new SpanContext(traceId, spanId, new SpanId(0), flags);
        }

        private IReadOnlyDictionary<string, string> CreateChildBaggage()
        {
            Dictionary<string, string> baggage = null;

            if (_references != null)
            {
                // optimization for 99% use cases, when there is only one parent
                if (_references.Count == 1)
                {
                    return _references[0].Context.Baggage;
                }

                foreach (Reference reference in _references)
                {
                    foreach (var baggageItem in reference.Context.GetBaggageItems())
                    {
                        if (baggage == null)
                        {
                            baggage = new Dictionary<string, string>();
                        }
                        baggage[baggageItem.Key] = baggageItem.Value;
                    }
                }
            }

            return baggage ?? SpanContext.EmptyBaggage;
        }

        private SpanContext CreateChildContext()
        {
            SpanContext preferredReference = PreferredReference();

            if (IsRpcServer())
            {
                if (IsSampled())
                {
                    _tracer.Metrics.TracesJoinedSampled.Inc(1);
                }
                else
                {
                    _tracer.Metrics.TracesJoinedNotSampled.Inc(1);
                }

                // Zipkin server compatibility
                if (_tracer.ZipkinSharedRpcSpan)
                {
                    return preferredReference;
                }
            }

            return new SpanContext(
                preferredReference.TraceId,
                SpanId.NewUniqueId(),
                preferredReference.SpanId,
                // should we do OR across passed references?
                preferredReference.Flags,
                CreateChildBaggage(),
                null);
        }

        //Visible for testing
        internal bool IsRpcServer()
        {
            return _tags.TryGetValue(Tags.SpanKind.Key, out object value)
                && value is string spanKind
                && string.Equals(Tags.SpanKindServer, spanKind, StringComparison.Ordinal);
        }

        private SpanContext PreferredReference()
        {
            Reference preferredReference = _references[0];
            foreach (Reference reference in _references)
            {
                // child_of takes precedence as a preferred parent
                if (string.Equals(References.ChildOf, reference.Type, StringComparison.Ordinal)
                    && !string.Equals(References.ChildOf, preferredReference.Type, StringComparison.Ordinal))
                {
                    preferredReference = reference;
                    break;
                }
            }
            return preferredReference.Context;
        }

        private bool IsSampled()
        {
            if (_references != null)
            {
                foreach (Reference reference in _references)
                {
                    if (reference.Context.IsSampled)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string DebugId()
        {
            if (_references?.Count == 1 && _references[0].Context.IsDebugIdContainerOnly())
            {
                return _references[0].Context.DebugId;
            }
            return null;
        }
    }
}