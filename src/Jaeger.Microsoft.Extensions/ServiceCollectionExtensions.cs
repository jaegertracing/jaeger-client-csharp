using System;
using System.Reflection;
using Jaeger.Core;
using Jaeger.Core.Baggage;
using Jaeger.Core.Metrics;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using Jaeger.Core.Util;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using JaegerConfiguration = Jaeger.Core.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJaeger(this IServiceCollection services, Action<Tracer.Builder> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ITracer>(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                // The default JaegerConfiguration throws if there is no ServiceName in an environment variable
                // so we do this separately and provide a default.
                string serviceName = JaegerConfiguration.GetProperty(JaegerConfiguration.JaegerServiceName);
                if (string.IsNullOrEmpty(serviceName))
                {
                    serviceName = Assembly.GetEntryAssembly().GetName().Name;
                }

                // Load everything from environment variables.
                // This will use the default configuration if there's no environment variables.
                var configuration = new JaegerConfiguration(serviceName, loggerFactory)
                    .WithTracerTags(JaegerConfiguration.TracerTagsFromEnv(loggerFactory))
                    .WithReporter(JaegerConfiguration.ReporterConfiguration.FromEnv(loggerFactory))
                    .WithSampler(JaegerConfiguration.SamplerConfiguration.FromEnv(loggerFactory))
                    .WithCodec(JaegerConfiguration.CodecConfiguration.FromEnv(loggerFactory));

                var tracerBuilder = configuration.GetTracerBuilder();

                // Allow user to change the builder in code.
                configure?.Invoke(tracerBuilder);

                ITracer tracer = tracerBuilder.Build();

                // Allows code that can't use DI to also access the tracer.
                GlobalTracer.Register(tracer);

                return tracer;
            });

            return services;
        }
    }
}
