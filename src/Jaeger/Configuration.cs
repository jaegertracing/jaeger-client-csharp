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
using Microsoft.Extensions.Configuration;
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
        public const string JaegerSamplerParam = JaegerPrefix + "SAMPLER_PARAM";

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
        /// Whether to use 128bit TraceID instead of 64bit.
        /// </summary>
        public const string JaegerTraceId128Bit = JaegerPrefix + "TRACEID_128BIT";

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
        public string ServiceName { get; }

        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private IMetricsFactory _metricsFactory;
        private Dictionary<string, string> _tracerTags;

        /// <summary>
        /// Lazy singleton <see cref="Tracer"/> initialized in <see cref="GetTracer()"/> method.
        /// </summary>
        private Tracer _tracer;

        public SamplerConfiguration SamplerConfig { get; private set; }
        public ReporterConfiguration ReporterConfig { get; private set; }
        public CodecConfiguration CodecConfig { get; private set; }
        public bool UseTraceId128Bit { get; private set; }
        public IReadOnlyDictionary<string, string> TracerTags => _tracerTags;

        public Configuration(string serviceName, ILoggerFactory loggerFactory)
        {
            ServiceName = Tracer.Builder.CheckValidServiceName(serviceName);
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<Configuration>();
        }

        /// <summary>
        /// Returns <see cref="Configuration"/> object from a Configuration.
        /// </summary>
        public static Configuration FromIConfiguration(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            ILogger logger = loggerFactory.CreateLogger<Configuration>();

            return new Configuration(GetProperty(JaegerServiceName, configuration), loggerFactory)
                .WithTracerTags(TracerTagsFromIConfiguration(logger, configuration))
                .WithTraceId128Bit(GetPropertyAsBool(JaegerTraceId128Bit, logger, configuration).GetValueOrDefault(false))
                .WithReporter(ReporterConfiguration.FromIConfiguration(loggerFactory, configuration))
                .WithSampler(SamplerConfiguration.FromIConfiguration(loggerFactory, configuration))
                .WithCodec(CodecConfiguration.FromIConfiguration(loggerFactory, configuration));
        }

        /// <summary>
        /// Returns <see cref="Configuration"/> object from environmental variables.
        /// </summary>
        public static Configuration FromEnv(ILoggerFactory loggerFactory)
        {
            var configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables().Build();

            return FromIConfiguration(loggerFactory, configuration);
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
            if (CodecConfig == null)
            {
                CodecConfig = new CodecConfiguration(_loggerFactory);
            }
            if (_metricsFactory == null)
            {
                _metricsFactory = NoopMetricsFactory.Instance;
            }
            IMetrics metrics = new MetricsImpl(_metricsFactory);
            IReporter reporter = ReporterConfig.GetReporter(metrics);
            ISampler sampler = SamplerConfig.GetSampler(ServiceName, metrics);
            Tracer.Builder builder = new Tracer.Builder(ServiceName)
                .WithLoggerFactory(_loggerFactory)
                .WithSampler(sampler)
                .WithReporter(reporter)
                .WithMetrics(metrics)
                .WithTags(_tracerTags);

            if (UseTraceId128Bit)
            {
                builder = builder.WithTraceId128Bit();
            }

            CodecConfig.Apply(builder);

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
            CodecConfig = codecConfig;
            return this;
        }

        public Configuration WithTraceId128Bit(bool useTraceId128Bit)
        {
            UseTraceId128Bit = useTraceId128Bit;
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

        [Obsolete("Use the property 'TracerTags' instead.")]
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

            /// <summary>
            /// Attempts to create a new <see cref="SamplerConfiguration"/> based on an IConfiguration.
            /// </summary>
            public static SamplerConfiguration FromIConfiguration(ILoggerFactory loggerFactory, IConfiguration configuration)
            {
                ILogger logger = loggerFactory.CreateLogger<Configuration>();

                return new SamplerConfiguration(loggerFactory)
                    .WithType(GetProperty(JaegerSamplerType, configuration))
                    .WithParam(GetPropertyAsDouble(JaegerSamplerParam, logger, configuration))
                    .WithManagerHostPort(GetProperty(JaegerSamplerManagerHostPort, configuration));
            }

            /// <summary>
            /// Attempts to create a new <see cref="SamplerConfiguration"/> based on the environment variables.
            /// </summary>
            public static SamplerConfiguration FromEnv(ILoggerFactory loggerFactory)
            {
                var configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables().Build();

                return FromIConfiguration(loggerFactory, configuration);
            }

            public virtual ISampler GetSampler(string serviceName, IMetrics metrics)
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
            private readonly ILogger _logger;
            private readonly IDictionary<IFormat<ITextMap>, List<Codec<ITextMap>>> _codecs;

            public CodecConfiguration(ILoggerFactory loggerFactory)
            {
                _logger = loggerFactory.CreateLogger<Configuration>();
                _codecs = new Dictionary<IFormat<ITextMap>, List<Codec<ITextMap>>>();
            }

            /// <summary>
            /// Attempts to create a new <see cref="CodecConfiguration"/> based on an IConfiguration.
            /// </summary>
            public static CodecConfiguration FromIConfiguration(ILoggerFactory loggerFactory, IConfiguration configuration)
            {
                ILogger logger = loggerFactory.CreateLogger<Configuration>();

                CodecConfiguration codecConfiguration = new CodecConfiguration(loggerFactory);
                string propagation = GetProperty(JaegerPropagation, configuration);
                if (propagation != null)
                {
                    foreach (string format in propagation.Split(','))
                    {
                        if (Enum.TryParse<Propagation>(format, true, out var propagationEnum))
                        {
                            codecConfiguration.WithPropagation(propagationEnum);
                        }
                        else
                        {
                            logger.LogError("Unknown propagation format {format}", format);
                        }
                    }
                }
                return codecConfiguration;
            }

            /// <summary>
            /// Attempts to create a new <see cref="CodecConfiguration"/> based on the environment variables.
            /// </summary>
            public static CodecConfiguration FromEnv(ILoggerFactory loggerFactory)
            {
                var configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables().Build();

                return FromIConfiguration(loggerFactory, configuration);
            }

            public CodecConfiguration WithPropagation(Propagation propagation)
            {
                switch (propagation)
                {
                    case Propagation.Jaeger:
                        AddCodec(BuiltinFormats.HttpHeaders, new TextMapCodec(true));
                        AddCodec(BuiltinFormats.TextMap, new TextMapCodec(false));
                        break;
                    case Propagation.B3:
                        AddCodec(BuiltinFormats.HttpHeaders, new B3TextMapCodec());
                        AddCodec(BuiltinFormats.TextMap, new B3TextMapCodec());
                        break;
                    default:
                        _logger.LogError("Unhandled propagation {propagation}", propagation);
                        break;
                }
                return this;
            }

            private void AddCodec(IFormat<ITextMap> format, Codec<ITextMap> codec)
            {
                List<Codec<ITextMap>> codecList;
                if (!_codecs.TryGetValue(format, out codecList))
                {
                    codecList = new List<Codec<ITextMap>>();
                    _codecs.Add(format, codecList);
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

            /// <summary>
            /// Attempts to create a new <see cref="ReporterConfiguration"/> based on an IConfiguration.
            /// </summary>
            public static ReporterConfiguration FromIConfiguration(ILoggerFactory loggerFactory, IConfiguration configuration)
            {
                ILogger logger = loggerFactory.CreateLogger<Configuration>();

                return new ReporterConfiguration(loggerFactory)
                    .WithLogSpans(GetPropertyAsBool(JaegerReporterLogSpans, logger, configuration).GetValueOrDefault(false))
                    .WithFlushInterval(GetPropertyAsTimeSpan(JaegerReporterFlushInterval, logger, configuration))
                    .WithMaxQueueSize(GetPropertyAsInt(JaegerReporterMaxQueueSize, logger, configuration))
                    .WithSender(SenderConfiguration.FromIConfiguration(loggerFactory, configuration));
            }

            /// <summary>
            /// Attempts to create a new <see cref="ReporterConfiguration"/> based on the environment variables.
            /// </summary>
            public static ReporterConfiguration FromEnv(ILoggerFactory loggerFactory)
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables().Build();

                return FromIConfiguration(loggerFactory, configuration);
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

            public virtual IReporter GetReporter(IMetrics metrics)
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
            public virtual ISender GetSender()
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
            /// Attempts to create a new <see cref="SenderConfiguration"/> based on an IConfiguration.
            /// </summary>
            public static SenderConfiguration FromIConfiguration(ILoggerFactory loggerFactory, IConfiguration configuration)
            {
                ILogger logger = loggerFactory.CreateLogger<Configuration>();

                string agentHost = GetProperty(JaegerAgentHost, configuration);
                int? agentPort = GetPropertyAsInt(JaegerAgentPort, logger, configuration);

                string collectorEndpoint = GetProperty(JaegerEndpoint, configuration);
                string authToken = GetProperty(JaegerAuthToken, configuration);
                string authUsername = GetProperty(JaegerUser, configuration);
                string authPassword = GetProperty(JaegerPassword, configuration);

                return new SenderConfiguration(loggerFactory)
                    .WithAgentHost(agentHost)
                    .WithAgentPort(agentPort)
                    .WithEndpoint(collectorEndpoint)
                    .WithAuthToken(authToken)
                    .WithAuthUsername(authUsername)
                    .WithAuthPassword(authPassword);
            }

            /// <summary>
            /// Attempts to create a new <see cref="SenderConfiguration"/> based on the environment variables.
            /// </summary>
            public static SenderConfiguration FromEnv(ILoggerFactory loggerFactory)
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables().Build();

                return FromIConfiguration(loggerFactory, configuration);
            }
        }

        private static string StringOrDefault(string value, string defaultValue)
        {
            return value != null && value.Length > 0 ? value : defaultValue;
        }

        private static string GetProperty(string name, IConfiguration configuration)
        {
            return configuration[name];
        }

        private static int? GetPropertyAsInt(string name, ILogger logger, IConfiguration configuration)
        {
            string value = GetProperty(name, configuration);
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

        private static double? GetPropertyAsDouble(string name, ILogger logger, IConfiguration configuration)
        {
            string value = GetProperty(name, configuration);
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

        private static TimeSpan? GetPropertyAsTimeSpan(string name, ILogger logger, IConfiguration configuration)
        {
            int? valueInMs = GetPropertyAsInt(name, logger, configuration);
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
        private static bool? GetPropertyAsBool(string name, ILogger logger, IConfiguration configuration)
        {
            string value = GetProperty(name, configuration);
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

        private static Dictionary<string, string> TracerTagsFromIConfiguration(ILogger logger, IConfiguration configuration)
        {
            Dictionary<string, string> tracerTagMaps = null;
            string tracerTags = GetProperty(JaegerTags, configuration);
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
                        tracerTagMaps[tagValue[0].Trim()] = ResolveValue(tagValue[1].Trim(), configuration);
                    }
                    else
                    {
                        logger.LogError("Tracer tag incorrectly formatted {tag}", tag);
                    }
                }
            }
            return tracerTagMaps;
        }

        private static string ResolveValue(string value, IConfiguration configuration)
        {
            if (value.StartsWith("${") && value.EndsWith("}"))
            {
                string[] kvp = value.Substring(2, value.Length - 3).Split(':');
                if (kvp.Length > 0)
                {
                    string propertyValue = GetProperty(kvp[0].Trim(), configuration);
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
