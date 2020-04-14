using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Baggage;
using Jaeger.Metrics;
using Jaeger.Propagation;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;

namespace Jaeger
{
    public class Tracer : ITracer, IDisposable
    {
        private readonly BaggageSetter _baggageSetter;
        private bool _isClosed = false;

        public string ServiceName { get; }
        public IReporter Reporter { get; }
        public ISampler Sampler { get; }
        internal PropagationRegistry Registry { get; }
        public IClock Clock { get; }
        public IMetrics Metrics { get; }
        public ILogger Logger { get; }

        public IScopeManager ScopeManager { get; }
        public ISpan ActiveSpan => ScopeManager.Active?.Span;

        public IReadOnlyDictionary<string, object> Tags { get; }

        public string Version { get; }
        public bool ZipkinSharedRpcSpan { get; }
        public bool ExpandExceptionLogs { get; }
        public bool UseTraceId128Bit { get; }
        public long IPv4 { get; }

        private Tracer(
            string serviceName,
            IReporter reporter,
            ISampler sampler,
            PropagationRegistry registry,
            IClock clock,
            IMetrics metrics,
            ILoggerFactory loggerFactory,
            Dictionary<string, object> tags,
            bool zipkinSharedRpcSpan,
            IScopeManager scopeManager,
            IBaggageRestrictionManager baggageRestrictionManager,
            bool expandExceptionLogs,
            bool useTraceId128Bit)
        {
            ServiceName = serviceName;
            Reporter = reporter;
            Sampler = sampler;
            Registry = registry;
            Clock = clock;
            Metrics = metrics;
            Logger = loggerFactory.CreateLogger<Tracer>();
            ZipkinSharedRpcSpan = zipkinSharedRpcSpan;
            ScopeManager = scopeManager;
            _baggageSetter = new BaggageSetter(baggageRestrictionManager, metrics);
            ExpandExceptionLogs = expandExceptionLogs;
            UseTraceId128Bit = useTraceId128Bit;

            Version = LoadVersion();
            tags[Constants.JaegerClientVersionTagKey] = Version;

            string hostname = GetHostName();
            if (!tags.ContainsKey(Constants.TracerHostnameTagKey))
            {
                if (hostname != null)
                {
                    tags[Constants.TracerHostnameTagKey] = hostname;
                }
            }

            if (tags.TryGetValue(Constants.TracerIpTagKey, out object ipTag))
            {
                try
                {
                    IPv4 = Utils.IpToInt(ipTag as string);
                }
                catch
                {
                }
            }
            else
            {
                try
                {
                    IPAddress hostIPv4 = Dns.GetHostAddresses(hostname).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                    tags[Constants.TracerIpTagKey] = hostIPv4.ToString();
                    IPv4 = Utils.IpToInt(hostIPv4);
                }
                catch
                {
                }
            }

            Tags = tags;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(nameof(Tracer));
            sb.Append('(');
            sb.Append($"ServiceName={ServiceName}, ");
            sb.Append($"Version={Version}, ");
            sb.Append($"Reporter={Reporter}, ");
            sb.Append($"Sampler={Sampler}, ");
            sb.Append($"IPv4={IPv4}, ");
            sb.Append($"Tags={string.Join(", ", Tags)}, ");
            sb.Append($"ZipkinSharedRpcSpan={ZipkinSharedRpcSpan}, ");
            sb.Append($"ExpandExceptionLogs={ExpandExceptionLogs}, ");
            sb.Append($"UseTraceId128Bit={UseTraceId128Bit}");
            sb.Append(')');
            return sb.ToString();
        }

        public void ReportSpan(Span span)
        {
            Reporter.Report(span);
            Metrics.SpansFinished.Inc(1);
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            IInjector injector = Registry.GetInjector(format);
            if (injector == null)
            {
                throw new NotSupportedException($"Unsupported format '{format}'");
            }
            injector.Inject((SpanContext)spanContext, carrier);
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            IExtractor extractor = Registry.GetExtractor(format);
            if (extractor == null)
            {
                throw new NotSupportedException($"Unsupported format '{format}'");
            }
            return extractor.Extract(carrier);
        }

        public SpanContext SetBaggage(Span span, string key, string value)
        {
            return _baggageSetter.SetBaggage(span, key, value);
        }

        /// <summary>
        /// Shuts down the <see cref="IReporter"/> and <see cref="ISampler"/>.
        /// </summary>
        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            if (!_isClosed)
            {
                await Reporter.CloseAsync(cancellationToken).ConfigureAwait(false);
                Sampler.Close();
                _isClosed = true;
            }
        }

        public void Dispose()
        {
            CancellationToken disposeTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token;

            CloseAsync(disposeTimeout).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        internal string GetHostName()
        {
            return Dns.GetHostName();
        }

        private static string LoadVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return $"CSharp-{version}";
        }

        public sealed class Builder
        {
            private PropagationRegistry _registry;
            private readonly Dictionary<string, object> _tags = new Dictionary<string, object>();

            // We need the loggerFactory for the PropagationRegistry so we have to defer these invocations.
            private readonly List<Action<PropagationRegistry>> _registryActions = new List<Action<PropagationRegistry>>();

            public string ServiceName { get; }
            public ILoggerFactory LoggerFactory { get; private set; }
            public IBaggageRestrictionManager BaggageRestrictionManager { get; private set; } = new DefaultBaggageRestrictionManager();
            public IMetrics Metrics { get; private set; } = new MetricsImpl(NoopMetricsFactory.Instance);
            public bool ZipkinSharedRpcSpan { get; private set; }
            public ISampler Sampler { get; private set; }
            public IReporter Reporter { get; private set; }
            public IClock Clock { get; private set; } = new SystemClock();
            public IScopeManager ScopeManager { get; private set; } = new AsyncLocalScopeManager();
            public bool ExpandExceptionLogs { get; private set; }
            public bool UseTraceId128Bit { get; private set; }
            public IReadOnlyDictionary<string, object> Tags => _tags;

            public Builder(string serviceName)
            {
                ServiceName = CheckValidServiceName(serviceName);

                _registryActions.Add(registry =>
                {
                    _registry.Register(BuiltinFormats.TextMap, new TextMapCodec(urlEncoding: false));
                    _registry.Register(BuiltinFormats.HttpHeaders, new TextMapCodec(urlEncoding: true));
                });
            }

            public Builder WithLoggerFactory(ILoggerFactory loggerFactory)
            {
                LoggerFactory = loggerFactory;
                return this;
            }

            public Builder WithReporter(IReporter reporter)
            {
                Reporter = reporter;
                return this;
            }

            public Builder WithSampler(ISampler sampler)
            {
                Sampler = sampler;
                return this;
            }
            public Builder RegisterInjector<TCarrier>(IFormat<TCarrier> format, Injector<TCarrier> injector)
            {
                _registryActions.Add(registry => _registry.Register(format, injector));
                return this;
            }

            public Builder RegisterExtractor<TCarrier>(IFormat<TCarrier> format, Extractor<TCarrier> extractor)
            {
                _registryActions.Add(registry => _registry.Register(format, extractor));
                return this;
            }

            public Builder RegisterCodec<TCarrier>(IFormat<TCarrier> format, Codec<TCarrier> codec)
            {
                _registryActions.Add(registry => _registry.Register(format, codec));
                return this;
            }

            public Builder WithMetrics(IMetrics metrics)
            {
                Metrics = metrics;
                return this;
            }

            public Builder WithMetricsFactory(IMetricsFactory factory)
            {
                Metrics = new MetricsImpl(factory);
                return this;
            }

            public Builder WithScopeManager(IScopeManager scopeManager)
            {
                ScopeManager = scopeManager;
                return this;
            }

            public Builder WithClock(IClock clock)
            {
                Clock = clock;
                return this;
            }

            public Builder WithZipkinSharedRpcSpan()
            {
                ZipkinSharedRpcSpan = true;
                return this;
            }

            public Builder WithExpandExceptionLogs()
            {
                ExpandExceptionLogs = true;
                return this;
            }

            public Builder WithTraceId128Bit()
            {
                UseTraceId128Bit = true;
                return this;
            }

            public Builder WithTag(string key, bool value)
            {
                _tags[key] = value;
                return this;
            }

            public Builder WithTag(string key, double value)
            {
                _tags[key] = value;
                return this;
            }

            public Builder WithTag(string key, int value)
            {
                _tags[key] = value;
                return this;
            }

            public Builder WithTag(string key, string value)
            {
                _tags[key] = value;
                return this;
            }

            public Builder WithTags(IEnumerable<KeyValuePair<string, string>> tags)
            {
                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        _tags[tag.Key] = tag.Value;
                    }
                }
                return this;
            }

            public Builder WithBaggageRestrictionManager(IBaggageRestrictionManager baggageRestrictionManager)
            {
                BaggageRestrictionManager = baggageRestrictionManager;
                return this;
            }

            public Tracer Build()
            {
                if (LoggerFactory == null)
                {
                    LoggerFactory = NullLoggerFactory.Instance;
                }

                _registry = new PropagationRegistry(LoggerFactory);
                foreach (var configureRegistry in _registryActions)
                {
                    configureRegistry(_registry);
                }

                if (Metrics == null)
                {
                    Metrics = new MetricsImpl(NoopMetricsFactory.Instance);
                }

                if (Reporter == null)
                {
                    Reporter = new RemoteReporter.Builder()
                        .WithLoggerFactory(LoggerFactory)
                        .WithMetrics(Metrics)
                        .Build();
                }
                if (Sampler == null)
                {
                    Sampler = new RemoteControlledSampler.Builder(ServiceName)
                        .WithLoggerFactory(LoggerFactory)
                        .WithMetrics(Metrics)
                        .Build();
                }

                return new Tracer(ServiceName, Reporter, Sampler, _registry, Clock, Metrics, LoggerFactory,
                    _tags, ZipkinSharedRpcSpan, ScopeManager, BaggageRestrictionManager, ExpandExceptionLogs, UseTraceId128Bit);
            }

            public static String CheckValidServiceName(String serviceName)
            {
                if (string.IsNullOrWhiteSpace(serviceName))
                {
                    throw new ArgumentException("Service name must not be null or empty");
                }
                return serviceName;
            }
        }
    }
}