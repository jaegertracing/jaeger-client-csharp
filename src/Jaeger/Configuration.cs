using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Jaeger.Metrics;
using Jaeger.Propagation;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Propagation;

namespace Jaeger
{
    /// <summary>
    /// This class is designed to provide <see cref="Tracer"/> or <see cref="Tracer.Builder"/> when Jaeger client
    /// configuration is provided in environmental variables. It also simplifies creation
    /// of the client from configuration files.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Prefix for all properties used to configure the Jaeger tracer.
        /// </summary>
        public const string JaegerPrefix = "JAEGER_";

        /// <summary>
        /// The full URL to the "traces" endpoint, like https://jaeger-collector:14268/api/traces.
        /// </summary>
        public const string JaegerEndpoint = JaegerPrefix + "ENDPOINT";

        /// <summary>
        /// The Auth Token to be added as "Bearer" on Authorization headers for requests sent to the endpoint.
        /// </summary>
        public const string JaegerAuthToken = JaegerPrefix + "AUTH_TOKEN";

        /// <summary>
        /// The Basic Auth username to be added on Authorization headers for requests sent to the endpoint.
        /// </summary>
        public const string JaegerUser = JaegerPrefix + "USER";

        /// <summary>
        /// The Basic Auth password to be added on Authorization headers for requests sent to the endpoint.
        /// </summary>
        public const string JaegerPassword = JaegerPrefix + "PASSWORD";

        /// <summary>
        /// The host name used to locate the agent.
        /// </summary>
        public const string JaegerAgentHost = JaegerPrefix + "AGENT_HOST";

        /// <summary>
        /// The port used to locate the agent.
        /// </summary>
        public const string JaegerAgentPort = JaegerPrefix + "AGENT_PORT";

        /// <summary>
        /// Whether the reporter should log the spans.
        /// </summary>
        public const string JaegerReporterLogSpans = JaegerPrefix + "REPORTER_LOG_SPANS";

        /// <summary>
        /// The maximum queue size for use when reporting spans remotely.
        /// </summary>
        public const string JaegerReporterMaxQueueSize = JaegerPrefix + "REPORTER_MAX_QUEUE_SIZE";

        /// <summary>
        /// The flush interval when reporting spans remotely.
        /// </summary>
        public const string JaegerReporterFlushInterval = JaegerPrefix + "REPORTER_FLUSH_INTERVAL";

        /// <summary>
        /// The sampler type.
        /// </summary>
        public const string JaegerSamplerType = JaegerPrefix + "SAMPLER_TYPE";

        /// <summary>
        /// The sampler parameter (number).
        /// </summary>
        public const string JaegerSamplerParam = "JAEGER_SAMPLER_PARAM";

        /// <summary>
        /// The sampler manager host:port.
        /// </summary>
        public const string JaegerSamplerManagerHostPort = JaegerPrefix + "SAMPLER_MANAGER_HOST_PORT";

        /// <summary>
        /// The service name.
        /// </summary>
        public const string JaegerServiceName = JaegerPrefix + "SERVICE_NAME";

        /// <summary>
        /// The tracer level tags.
        /// </summary>
        public const string JaegerTags = JaegerPrefix + "TAGS";

        /// <summary>
        /// Comma separated list of formats to use for propagating the trace context. Default will the
        /// standard Jaeger format. Valid values are jaeger and b3.
        /// </summary>
        public const string JaegerPropagation = JaegerPrefix + "PROPAGATION";

        /// <summary>
        /// The supported trace context propagation formats.
        /// </summary>
        public enum Propagation
        {
            /// <summary>
            /// The default Jaeger trace context propagation format.
            /// </summary>
            Jaeger,

            /// <summary>
            /// The Zipkin B3 trace context propagation format.
            /// </summary>
            B3
        }

        private readonly object _lock = new object();

        /// <summary>
        /// The serviceName that the tracer will use.
        /// </summary>
        private readonly string _serviceName;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private CodecConfiguration _codecConfig;
        private IMetricsFactory _metricsFactory;
        private Dictionary<string, string> _tracerTags;

        /// <summary>
        /// Lazy singleton <see cref="Tracer"/> initialized in <see cref="GetTracer()"/> method.
        /// </summary>
        private Tracer _tracer;

        public SamplerConfiguration SamplerConfig { get; private set; }
        public ReporterConfiguration ReporterConfig { get; private set; }

        public Configuration(string serviceName, ILoggerFactory loggerFactory)
        {
            _serviceName = Tracer.Builder.CheckValidServiceName(serviceName);
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<Configuration>();
        }

        /// <summary>
        /// Returns <see cref="Configuration"/> object from environmental variables.
        /// </summary>
        public static Configuration FromEnv(ILoggerFactory loggerFactory)
        {
            ILogger logger = loggerFactory.CreateLogger<Configuration>();

            return new Configuration(GetProperty(JaegerServiceName), loggerFactory)
                .WithTracerTags(TracerTagsFromEnv(logger))
                .WithReporter(ReporterConfiguration.FromEnv(loggerFactory))
                .WithSampler(SamplerConfiguration.FromEnv(loggerFactory))
                .WithCodec(CodecConfiguration.FromEnv(loggerFactory));
        }

        public Tracer.Builder GetTracerBuilder()
        {
            if (ReporterConfig == null)
            {
                ReporterConfig = new ReporterConfiguration(_loggerFactory);
            }
            if (SamplerConfig == null)
            {
                SamplerConfig = new SamplerConfiguration(_loggerFactory);
            }
            if (_codecConfig == null)
            {
                _codecConfig = new CodecConfiguration(new Dictionary<IFormat<ITextMap>, List<Codec<ITextMap>>>());
            }
            if (_metricsFactory == null)
            {
                _metricsFactory = NoopMetricsFactory.Instance;
            }
            IMetrics metrics = new MetricsImpl(_metricsFactory);
            IReporter reporter = ReporterConfig.GetReporter(metrics);
            ISampler sampler = SamplerConfig.CreateSampler(_serviceName, metrics);
            Tracer.Builder builder = new Tracer.Builder(_serviceName)
                .WithLoggerFactory(_loggerFactory)
                .WithSampler(sampler)
                .WithReporter(reporter)
                .WithMetrics(metrics)
                .WithTags(_tracerTags);

            _codecConfig.Apply(builder);

            return builder;
        }

        public ITracer GetTracer()
        {
            lock (_lock)
            {
                if (_tracer != null)
                {
                    return _tracer;
                }

                _tracer = GetTracerBuilder().Build();
                _logger.LogInformation("Initialized {tracer}", _tracer);

                return _tracer;
            }
        }

        public async Task CloseTracerAsync(CancellationToken cancellationToken)
        {
            Tracer tracer;
            lock (_lock)
            {
                tracer = _tracer;
            }

            if (tracer != null)
            {
                await tracer.CloseAsync(cancellationToken);
            }
        }

        public void WithMetricsFactory(IMetricsFactory metricsFactory)
        {
            _metricsFactory = metricsFactory;
        }

        public Configuration WithReporter(ReporterConfiguration reporterConfig)
        {
            ReporterConfig = reporterConfig;
            return this;
        }

        public Configuration WithSampler(SamplerConfiguration samplerConfig)
        {
            SamplerConfig = samplerConfig;
            return this;
        }

        public Configuration WithCodec(CodecConfiguration codecConfig)
        {
            _codecConfig = codecConfig;
            return this;
        }

        public Configuration WithTracerTags(Dictionary<string, string> tracerTags)
        {
            if (tracerTags != null)
            {
                _tracerTags = new Dictionary<string, string>(tracerTags);
            }
            return this;
        }

        public IReadOnlyDictionary<string, string> GetTracerTags()
        {
            return _tracerTags;
        }

        /// <summary>
        /// SamplerConfiguration allows to configure which sampler the tracer will use.
        /// </summary>
        public class SamplerConfiguration
        {
            private readonly ILoggerFactory _loggerFactory;

            /// <summary>
            /// The type of sampler to use in the tracer. Optional. Valid values: remote (default),
            /// ratelimiting, probabilistic, const.
            /// </summary>
            public string Type { get; private set; }

            /// <summary>
            /// The integer or floating point value that makes sense for the correct samplerType. Optional.
            /// </summary>
            public double? Param { get; private set; }

            /// <summary>
            /// HTTP host:port of the sampling manager that can provide sampling strategy to this service.
            /// Optional.
            /// </summary>
            public string ManagerHostPort { get; private set; }

            public SamplerConfiguration(ILoggerFactory loggerFactory)
            {
                _loggerFactory = loggerFactory;
            }

            public static SamplerConfiguration FromEnv(ILoggerFactory loggerFactory)
            {
                ILogger logger = loggerFactory.CreateLogger<Configuration>();

                return new SamplerConfiguration(loggerFactory)
                    .WithType(GetProperty(JaegerSamplerType))
                    .WithParam(GetPropertyAsDouble(JaegerSamplerParam, logger))
                    .WithManagerHostPort(GetProperty(JaegerSamplerManagerHostPort));
            }

            // for tests
            internal ISampler CreateSampler(string serviceName, IMetrics metrics)
            {
                string samplerType = StringOrDefault(Type, RemoteControlledSampler.Type);
                double samplerParam = Param.GetValueOrDefault(ProbabilisticSampler.DefaultSamplingProbability);
                string hostPort = StringOrDefault(ManagerHostPort, HttpSamplingManager.DefaultHostPort);

                switch (samplerType)
                {
                    case ConstSampler.Type:
                        return new ConstSampler(samplerParam != 0);
                    case ProbabilisticSampler.Type:
                        return new ProbabilisticSampler(samplerParam);
                    case RateLimitingSampler.Type:
                        return new RateLimitingSampler(samplerParam);
                    case RemoteControlledSampler.Type:
                        return new RemoteControlledSampler.Builder(serviceName)
                            .WithLoggerFactory(_loggerFactory)
                            .WithSamplingManager(new HttpSamplingManager(hostPort))
                            .WithInitialSampler(new ProbabilisticSampler(samplerParam))
                            .WithMetrics(metrics)
                            .Build();
                    default:
                        throw new NotSupportedException($"Invalid sampling strategy {samplerType}");
                }
            }

            public SamplerConfiguration WithType(string type)
            {
                Type = type;
                return this;
            }

            public SamplerConfiguration WithParam(double? param)
            {
                Param = param;
                return this;
            }

            public SamplerConfiguration WithManagerHostPort(string managerHostPort)
            {
                ManagerHostPort = managerHostPort;
                return this;
            }
        }

        /// <summary>
        /// CodecConfiguration can be used to support additional trace context propagation codec.
        /// </summary>
        public class CodecConfiguration
        {
            private readonly IDictionary<IFormat<ITextMap>, List<Codec<ITextMap>>> _codecs;

            internal CodecConfiguration(IDictionary<IFormat<ITextMap>, List<Codec<ITextMap>>> codecs)
            {
                _codecs = codecs;
            }

            public static CodecConfiguration FromEnv(ILoggerFactory loggerFactory)
            {
                ILogger logger = loggerFactory.CreateLogger<Configuration>();

                var codecs = new Dictionary<IFormat<ITextMap>, List<Codec<ITextMap>>>();
                string propagation = GetProperty(JaegerPropagation);
                if (propagation != null)
                {
                    foreach (string format in propagation.Split(','))
                    {
                        if (Enum.TryParse<Propagation>(format, true, out var propagationEnum))
                        {
                            switch (propagationEnum)
                            {
                                case Propagation.Jaeger:
                                    AddCodec(codecs, BuiltinFormats.HttpHeaders, new TextMapCodec(true));
                                    AddCodec(codecs, BuiltinFormats.TextMap, new TextMapCodec(false));
                                    break;
                                case Propagation.B3:
                                    AddCodec(codecs, BuiltinFormats.HttpHeaders, new B3TextMapCodec());
                                    AddCodec(codecs, BuiltinFormats.TextMap, new B3TextMapCodec());
                                    break;
                                default:
                                    logger.LogError("Unhandled propagation format {format}", format);
                                    break;
                            }
                        }
                        else
                        {
                            logger.LogError("Unknown propagation format {format}", format);
                        }
                    }
                }
                return new CodecConfiguration(codecs);
            }

            private static void AddCodec(IDictionary<IFormat<ITextMap>, List<Codec<ITextMap>>> codecs, IFormat<ITextMap> format, Codec<ITextMap> codec)
            {
                List<Codec<ITextMap>> codecList;
                if (!codecs.TryGetValue(format, out codecList))
                {
                    codecList = new List<Codec<ITextMap>>();
                    codecs.Add(format, codecList);
                }
                codecList.Add(codec);
            }

            public void Apply(Tracer.Builder builder)
            {
                // Replace existing TEXT_MAP and HTTP_HEADERS codec with one that represents the
                // configured propagation formats
                RegisterCodec(builder, BuiltinFormats.HttpHeaders);
                RegisterCodec(builder, BuiltinFormats.TextMap);
            }

            protected void RegisterCodec(Tracer.Builder builder, IFormat<ITextMap> format)
            {
                if (_codecs.ContainsKey(format))
                {
                    List<Codec<ITextMap>> codecsForFormat = _codecs[format];
                    Codec<ITextMap> codec = codecsForFormat.Count == 1
                        ? codecsForFormat[0]
                        : new CompositeCodec<ITextMap>(codecsForFormat);

                    builder.RegisterCodec(format, codec);
                }
            }
        }

        public class ReporterConfiguration
        {
            private readonly ILoggerFactory _loggerFactory;

            public bool LogSpans { get; private set; }
            public TimeSpan? FlushInterval { get; private set; }
            public int? MaxQueueSize { get; private set; }
            public SenderConfiguration SenderConfig { get; private set; }

            public ReporterConfiguration(ILoggerFactory loggerFactory)
            {
                _loggerFactory = loggerFactory;
                SenderConfig = new SenderConfiguration(loggerFactory);
            }

            public static ReporterConfiguration FromEnv(ILoggerFactory loggerFactory)
            {
                ILogger logger = loggerFactory.CreateLogger<Configuration>();

                return new ReporterConfiguration(loggerFactory)
                    .WithLogSpans(GetPropertyAsBool(JaegerReporterLogSpans, logger).GetValueOrDefault(false))
                    .WithFlushInterval(GetPropertyAsTimeSpan(JaegerReporterFlushInterval, logger))
                    .WithMaxQueueSize(GetPropertyAsInt(JaegerReporterMaxQueueSize, logger))
                    .WithSender(SenderConfiguration.FromEnv(loggerFactory));
            }

            public ReporterConfiguration WithLogSpans(Boolean logSpans)
            {
                LogSpans = logSpans;
                return this;
            }

            public ReporterConfiguration WithFlushInterval(TimeSpan? flushInterval)
            {
                FlushInterval = flushInterval;
                return this;
            }

            public ReporterConfiguration WithMaxQueueSize(int? maxQueueSize)
            {
                MaxQueueSize = maxQueueSize;
                return this;
            }

            public ReporterConfiguration WithSender(SenderConfiguration senderConfiguration)
            {
                SenderConfig = senderConfiguration;
                return this;
            }

            public IReporter GetReporter(IMetrics metrics)
            {
                IReporter reporter = new RemoteReporter.Builder()
                    .WithLoggerFactory(_loggerFactory)
                    .WithMetrics(metrics)
                    .WithSender(SenderConfig.GetSender())
                    .WithFlushInterval(FlushInterval.GetValueOrDefault(RemoteReporter.DefaultFlushInterval))
                    .WithMaxQueueSize(MaxQueueSize.GetValueOrDefault(RemoteReporter.DefaultMaxQueueSize))
                    .Build();

                if (LogSpans)
                {
                    IReporter loggingReporter = new LoggingReporter(_loggerFactory);
                    reporter = new CompositeReporter(reporter, loggingReporter);
                }
                return reporter;
            }
        }

        /// <summary>
        /// Holds the configuration related to the sender. A sender can be a <see cref="HttpSender"/> or <see cref="UdpSender"/>.
        /// </summary>
        public class SenderConfiguration
        {
            private readonly ILoggerFactory _loggerFactory;
            private readonly ILogger _logger;

            /// <summary>
            /// A custom sender set by our consumers. If set, nothing else has effect. Optional.
            /// </summary>
            public ISender Sender { get; private set; }

            /// <summary>
            /// The Agent Host. Has no effect if the sender is set. Optional.
            /// </summary>
            public string AgentHost { get; private set; }

            /// <summary>
            /// The Agent Port. Has no effect if the sender is set. Optional.
            /// </summary>
            public int? AgentPort { get; private set; }

            /// <summary>
            /// The endpoint, like https://jaeger-collector:14268/api/traces.
            /// </summary>
            public string Endpoint { get; private set; }

            /// <summary>
            /// The Auth Token to be added as "Bearer" on Authorization headers for requests sent to the endpoint.
            /// </summary>
            public string AuthToken { get; private set; }

            /// <summary>
            /// The Basic Auth username to be added on Authorization headers for requests sent to the endpoint.
            /// </summary>
            public string AuthUsername { get; private set; }

            /// <summary>
            /// The Basic Auth password to be added on Authorization headers for requests sent to the endpoint.
            /// </summary>
            public string AuthPassword { get; private set; }

            public SenderConfiguration(ILoggerFactory loggerFactory)
            {
                _loggerFactory = loggerFactory;
                _logger = loggerFactory.CreateLogger<Configuration>();
            }

            public SenderConfiguration WithAgentHost(string agentHost)
            {
                AgentHost = agentHost;
                return this;
            }

            public SenderConfiguration WithAgentPort(int? agentPort)
            {
                AgentPort = agentPort;
                return this;
            }

            public SenderConfiguration WithEndpoint(string endpoint)
            {
                Endpoint = endpoint;
                return this;
            }

            public SenderConfiguration WithAuthToken(string authToken)
            {
                AuthToken = authToken;
                return this;
            }

            public SenderConfiguration WithAuthUsername(string username)
            {
                AuthUsername = username;
                return this;
            }

            public SenderConfiguration WithAuthPassword(string password)
            {
                AuthPassword = password;
                return this;
            }

            /// <summary>
            /// Returns a sender if one was given when creating the configuration, or attempts to create a sender based on the
            /// configuration's state.
            /// </summary>
            /// <returns>The sender passed via the constructor or a properly configured sender.</returns>
            public ISender GetSender()
            {
                // if we have a sender, that's the one we return
                if (Sender != null)
                {
                    return Sender;
                }

                if (!string.IsNullOrEmpty(Endpoint))
                {
                    HttpSender.Builder httpSenderBuilder = new HttpSender.Builder(Endpoint);
                    if (!string.IsNullOrEmpty(AuthUsername) && !string.IsNullOrEmpty(AuthPassword))
                    {
                        _logger.LogDebug("Using HTTP Basic authentication with data from the environment variables.");
                        httpSenderBuilder.WithAuth(AuthUsername, AuthPassword);
                    }
                    else if (!string.IsNullOrEmpty(AuthToken))
                    {
                        _logger.LogDebug("Auth Token environment variable found.");
                        httpSenderBuilder.WithAuth(AuthToken);
                    }

                    _logger.LogDebug("Using the HTTP Sender to send spans directly to the endpoint.");
                    return httpSenderBuilder.Build();
                }

                _logger.LogDebug("Using the UDP Sender to send spans to the agent.");
                return new UdpSender(
                        StringOrDefault(AgentHost, UdpSender.DefaultAgentUdpHost),
                        AgentPort.GetValueOrDefault(UdpSender.DefaultAgentUdpCompactPort),
                        0 /* max packet size */);
            }

            /// <summary>
            /// Attempts to create a new <see cref="SenderConfiguration"/> based on the environment variables.
            /// </summary>
            public static SenderConfiguration FromEnv(ILoggerFactory loggerFactory)
            {
                ILogger logger = loggerFactory.CreateLogger<Configuration>();

                string agentHost = GetProperty(JaegerAgentHost);
                int? agentPort = GetPropertyAsInt(JaegerAgentPort, logger);

                string collectorEndpoint = GetProperty(JaegerEndpoint);
                string authToken = GetProperty(JaegerAuthToken);
                string authUsername = GetProperty(JaegerUser);
                string authPassword = GetProperty(JaegerPassword);

                return new SenderConfiguration(loggerFactory)
                    .WithAgentHost(agentHost)
                    .WithAgentPort(agentPort)
                    .WithEndpoint(collectorEndpoint)
                    .WithAuthToken(authToken)
                    .WithAuthUsername(authUsername)
                    .WithAuthPassword(authPassword);
            }
        }

        private static string StringOrDefault(string value, string defaultValue)
        {
            return value != null && value.Length > 0 ? value : defaultValue;
        }

        private static string GetProperty(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        private static int? GetPropertyAsInt(string name, ILogger logger)
        {
            string value = GetProperty(name);
            if (!string.IsNullOrEmpty(value))
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
                {
                    return intValue;
                }
                else
                {
                    logger.LogError("Failed to parse integer for property {property} with value {value}", name, value);
                }
            }
            return null;
        }

        private static double? GetPropertyAsDouble(string name, ILogger logger)
        {
            string value = GetProperty(name);
            if (!string.IsNullOrEmpty(value))
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue))
                {
                    return doubleValue;
                }
                else
                {
                    logger.LogError("Failed to parse double for property {property} with value {value}", name, value);
                }
            }
            return null;
        }

        private static TimeSpan? GetPropertyAsTimeSpan(string name, ILogger logger)
        {
            int? valueInMs = GetPropertyAsInt(name, logger);
            if (valueInMs.HasValue)
            {
                return TimeSpan.FromMilliseconds(valueInMs.Value);
            }
            return null;
        }

        /// <summary>
        /// Gets the system property defined by the name, and returns a boolean value represented by
        /// the name. This method defaults to returning false for a name that doesn't exist.
        /// </summary>
        private static bool? GetPropertyAsBool(string name, ILogger logger)
        {
            string value = GetProperty(name);
            if (!string.IsNullOrEmpty(value))
            {
                if (string.Equals(value, "1", StringComparison.Ordinal))
                {
                    return true;
                }
                if (string.Equals(value, "0", StringComparison.Ordinal))
                {
                    return false;
                }

                if (bool.TryParse(value, out var boolValue))
                {
                    return boolValue;
                }
                else
                {
                    logger.LogError("Failed to parse bool for property {property} with value {value}", name, value);
                }
            }
            return null;
        }

        private static Dictionary<string, string> TracerTagsFromEnv(ILogger logger)
        {
            Dictionary<string, string> tracerTagMaps = null;
            string tracerTags = GetProperty(JaegerTags);
            if (!string.IsNullOrEmpty(tracerTags))
            {
                string[] tags = tracerTags.Split(',');
                foreach (string tag in tags)
                {
                    string[] tagValue = tag.Trim().Split('=');
                    if (tagValue.Length == 2)
                    {
                        if (tracerTagMaps == null)
                        {
                            tracerTagMaps = new Dictionary<string, string>();
                        }
                        tracerTagMaps[tagValue[0].Trim()] = ResolveValue(tagValue[1].Trim());
                    }
                    else
                    {
                        logger.LogError("Tracer tag incorrectly formatted {tag}", tag);
                    }
                }
            }
            return tracerTagMaps;
        }

        private static string ResolveValue(string value)
        {
            if (value.StartsWith("${") && value.EndsWith("}"))
            {
                string[] kvp = value.Substring(2, value.Length - 3).Split(':');
                if (kvp.Length > 0)
                {
                    string propertyValue = GetProperty(kvp[0].Trim());
                    if (propertyValue == null && kvp.Length > 1)
                    {
                        propertyValue = kvp[1].Trim();
                    }
                    return propertyValue;
                }
            }
            return value;
        }
    }
}
