using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using LetsTrace.Metrics;
using LetsTrace.Propagation;
using LetsTrace.Reporters;
using LetsTrace.Samplers;
using LetsTrace.Transport;
using LetsTrace.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;

namespace LetsTrace
{
    // Tracer is the main object that consumers use to start spans
    public class Tracer : ILetsTraceTracer
    {
        private readonly ILogger _logger;

        public IScopeManager ScopeManager { get; }
        public IClock Clock { get; }
        public ISpan ActiveSpan => ScopeManager.Active?.Span;
        public string HostIPv4 { get; }
        public string ServiceName { get; }
        public IDictionary<string, Field> Tags { get; }
        public IReporter Reporter { get; }
        public ISampler Sampler { get; }
        public IPropagationRegistry PropagationRegistry { get; }
        public IMetrics Metrics { get; }

        // TODO: support trace options
        private Tracer(string serviceName, IDictionary<string, Field> tags, IScopeManager scopeManager, ILoggerFactory loggerFactory,
            IPropagationRegistry propagationRegistry, ISampler sampler, IReporter reporter, IMetrics metrics)
        {
            ServiceName = serviceName;
            Tags = tags;
            ScopeManager = scopeManager;
            PropagationRegistry = propagationRegistry;
            Sampler = sampler;
            Reporter = reporter;
            Metrics = metrics;
            Clock = new Clock();

            _logger = loggerFactory.CreateLogger<Tracer>();

            if (tags.TryGetValue(Constants.TRACER_IP_TAG_KEY, out var field))
            {
                HostIPv4 = field.StringValue;
            } 
        }

        private static string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return $"CSharp-{version}";
        }

        public void Dispose()
        {
            Sampler?.Dispose();
            Reporter?.Dispose();
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName, Sampler, Metrics);
        }

        public void ReportSpan(ILetsTraceSpan span)
        {
            if (span.Context.IsSampled) {
                Reporter.Report(span);
                Metrics.SpansFinished.Inc(1);
            }
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            return PropagationRegistry.Extract(format, carrier);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            PropagationRegistry.Inject(spanContext, format, carrier);
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

        public sealed class Builder
        {
            private readonly string _serviceName;
            private readonly Dictionary<string, Field> _initialTags = new Dictionary<string, Field>();
            private ILoggerFactory _loggerFactory;
            private IScopeManager _scopeManager;
            private IPropagationRegistry _propagationRegistry;
            private ISamplingManager _samplingManager;
            private ISampler _sampler;
            private ITransport _transport;
            private IReporter _reporter;
            private IMetrics _metrics;

            public Builder(String serviceName)
            {
                this._serviceName = CheckValidServiceName(serviceName);

                var version = GetVersion();
                this.WithTag(Constants.LETSTRACE_CLIENT_VERSION_TAG_KEY, version);

                string hostname = System.Net.Dns.GetHostName();
                if (hostname != null)
                {
                    this.WithTag(Constants.TRACER_HOSTNAME_TAG_KEY, hostname);

                    try
                    {
                        var hostIPv4 = System.Net.Dns.GetHostAddresses(hostname).First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
                        this.WithTag(Constants.TRACER_IP_TAG_KEY, hostIPv4);
                    }
                    catch
                    {
                    }
                }
            }

            public Builder WithLoggerFactory(ILoggerFactory loggerFactory)
            {
                this._loggerFactory = loggerFactory;
                return this;
            }

            public Builder WithPropagationRegistry(IPropagationRegistry propagationRegistry)
            {
                this._propagationRegistry = propagationRegistry;
                return this;
            }

            public Builder WithReporter(IReporter reporter)
            {
                this._reporter = reporter;
                return this;
            }

            public Builder WithTransport(ITransport transport)
            {
                this._transport = transport;
                return this;
            }

            public Builder WithSampler(ISampler sampler)
            {
                this._sampler = sampler;
                return this;
            }

            public Builder WithSamplingManager(ISamplingManager samplingManager)
            {
                this._samplingManager = samplingManager;
                return this;
            }

            public Builder WithMetrics(IMetrics metrics)
            {
                this._metrics = metrics;
                return this;
            }

            public Builder WithMetricsFactory(IMetricsFactory factory)
            {
                this._metrics = factory.CreateMetrics();
                return this;
            }

            public Builder WithScopeManager(IScopeManager scopeManager)
            {
                this._scopeManager = scopeManager;
                return this;
            }

            public Builder WithTag(string key, bool value) => WithTag(key, new Field<bool> { Key = key, Value = value });

            public Builder WithTag(string key, double value) => WithTag(key, new Field<double> { Key = key, Value = value });

            public Builder WithTag(string key, int value) => WithTag(key, new Field<int> { Key = key, Value = value });

            public Builder WithTag(string key, string value) => WithTag(key, new Field<string> { Key = key, Value = value });

            private Builder WithTag(string key, Field value)
            {
                this._initialTags[key] = value;
                return this;
            }

            public Tracer Build()
            {
                if (_loggerFactory == null)
                {
                    _loggerFactory = NullLoggerFactory.Instance;
                }
                if (_metrics == null)
                {
                    _metrics = NoopMetricsFactory.Instance.CreateMetrics();
                }
                if (_reporter == null)
                {
                    if (_transport == null)
                    {
                        if (_loggerFactory == NullLoggerFactory.Instance)
                        {
                            // TODO: Technically, it would be fine to get rid of NullReporter since the NullLogger does the same.
                            // Check the performance penalty between using LoggingReporter with NullReporter compared to NullReporter!
                            _reporter = new NullReporter();
                        }
                        else
                        {
                            _reporter = new LoggingReporter(_loggerFactory);
                        }
                    }
                    else
                    {
                        _reporter = new RemoteReporter.Builder(_transport)
                            .WithLoggerFactory(_loggerFactory)
                            .WithMetrics(_metrics)
                            .Build();
                    }
                }
                if (_sampler == null)
                {
                    if (_samplingManager == null)
                    {
                        _sampler = new ConstSampler(true);
                    }
                    else
                    {
                        _sampler = new RemoteControlledSampler.Builder(_serviceName, _samplingManager)
                            .WithLoggerFactory(_loggerFactory)
                            .WithMetrics(_metrics)
                            .Build();
                    }
                }
                if (_scopeManager == null)
                {
                    _scopeManager = new AsyncLocalScopeManager();
                }
                if (_propagationRegistry == null)
                {
                    _propagationRegistry = Propagators.TextMap;
                }

                return new Tracer(_serviceName, _initialTags, _scopeManager, _loggerFactory, _propagationRegistry, _sampler, _reporter, _metrics);
            }

            private static string CheckValidServiceName(String serviceName)
            {
                if (string.IsNullOrEmpty(serviceName?.Trim()))
                {
                    throw new ArgumentException("Service name must not be null or empty", nameof(serviceName));
                }

                return serviceName;
            }
        }
    }
}