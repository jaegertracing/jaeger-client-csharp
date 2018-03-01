using System;
using System.Collections.Generic;
using System.Linq;
using LetsTrace.Propagation;
using LetsTrace.Reporters;
using LetsTrace.Samplers;
using LetsTrace.Util;
using OpenTracing;
using OpenTracing.Propagation;

namespace LetsTrace
{
    // Tracer is the main object that consumers use to start spans
    public class Tracer : ILetsTraceTracer
    {
        internal Dictionary<string, IInjector> _injectors { get; private set; } = new Dictionary<string, IInjector>();
        internal Dictionary<string, IExtractor> _extractors { get; private set; } = new Dictionary<string, IExtractor>();
        private IReporter _reporter;
        private ISampler _sampler;

        public IScopeManager ScopeManager { get; private set; }
        public IClock Clock { get; internal set; }
        public ISpan ActiveSpan { get { return ScopeManager.Active.Span; } }
        public string HostIPv4 { get; }
        public string ServiceName { get; }

        // TODO: support tracer level tags
        // TODO: support trace options
        // TODO: add logger
        public Tracer(string serviceName, IReporter reporter, string hostIPv4, ISampler sampler, IScopeManager scopeManager)
        {
            ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
            HostIPv4 = hostIPv4 ?? throw new ArgumentNullException(nameof(hostIPv4));
            _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
            ScopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));

            // set up default options - TODO: allow these to be overridden via options
            var defaultHeadersConfig = new HeadersConfig(Constants.TraceContextHeaderName, Constants.TraceBaggageHeaderPrefix);

            var textPropagator = TextMapPropagator.NewTextMapPropagator(defaultHeadersConfig);
            AddCodec(BuiltinFormats.TextMap.ToString(), textPropagator, textPropagator);

            var httpHeaderPropagator = TextMapPropagator.NewHTTPHeaderPropagator(defaultHeadersConfig);
            AddCodec(BuiltinFormats.HttpHeaders.ToString(), httpHeaderPropagator, httpHeaderPropagator);

            Clock = new Clock();
        }

        internal void AddCodec(string format, IInjector injector, IExtractor extractor)
        {
            if (!_injectors.ContainsKey(format)) {
                _injectors.Add(format, injector);
            }

            if (!_extractors.ContainsKey(format)) {
                _extractors.Add(format, extractor);
            }
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName, _sampler);
        }

        public void ReportSpan(ILetsTraceSpan span)
        {
            var context = span.Context as ILetsTraceSpanContext;
            if (context.IsSampled()) {
                _reporter.Report(span);
            }
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            if (_extractors.ContainsKey(format.ToString())) {
                return _extractors[format.ToString()].Extract(carrier);
            }
            throw new Exception($"{format.ToString()} is not a supported extraction format");
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            if (_injectors.ContainsKey(format.ToString())) {
                _injectors[format.ToString()].Inject(spanContext, carrier);
                return;
            }
            throw new Exception($"{format.ToString()} is not a supported injection format");
        }

        // TODO: setup baggage restriction
        public ILetsTraceSpan SetBaggageItem(ILetsTraceSpan span, string key, string value)
        {
            var context = (SpanContext)span.Context;
            var baggage = context.GetBaggageItems().ToDictionary(b => b.Key, b => b.Value);
            baggage[key] = value;
            context.SetBaggageItems(baggage);
            return span;
        }
    }
}