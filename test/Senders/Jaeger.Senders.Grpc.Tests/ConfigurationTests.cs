using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing.Noop;
using OpenTracing.Util;
using Xunit;

namespace Jaeger.Senders.Grpc.Tests
{
    public class ConfigurationTests : IDisposable
    {
        private const string TestProperty = "TestProperty";

        private readonly ILoggerFactory _loggerFactory;

        public ConfigurationTests()
        {
            _loggerFactory = NullLoggerFactory.Instance;
            Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(_loggerFactory)
                .RegisterSenderFactory<GrpcSenderFactory>();

            ClearProperties();
        }

        public void Dispose()
        {
            ClearProperties();
        }

        private void ClearProperties()
        {
            // Explicitly clear all properties
            ClearProperty(Configuration.JaegerAgentHost);
            ClearProperty(Configuration.JaegerAgentPort);
            ClearProperty(Configuration.JaegerGrpcTarget);
            ClearProperty(Configuration.JaegerGrpcRootCertificate);
            ClearProperty(Configuration.JaegerGrpcClientChain);
            ClearProperty(Configuration.JaegerGrpcClientKey);
            ClearProperty(Configuration.JaegerReporterLogSpans);
            ClearProperty(Configuration.JaegerReporterMaxQueueSize);
            ClearProperty(Configuration.JaegerReporterFlushInterval);
            ClearProperty(Configuration.JaegerSamplerType);
            ClearProperty(Configuration.JaegerSamplerParam);
            ClearProperty(Configuration.JaegerSamplerManagerHostPort);
            ClearProperty(Configuration.JaegerServiceName);
            ClearProperty(Configuration.JaegerTags);
            ClearProperty(Configuration.JaegerSenderFactory);
            ClearProperty(Configuration.JaegerTraceId128Bit);
            ClearProperty(Configuration.JaegerEndpoint);
            ClearProperty(Configuration.JaegerAuthToken);
            ClearProperty(Configuration.JaegerUser);
            ClearProperty(Configuration.JaegerPassword);
            ClearProperty(Configuration.JaegerPropagation);

            ClearProperty(TestProperty);

            // Reset opentracing's global tracer
            FieldInfo field = typeof(GlobalTracer).GetField("_tracer", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(GlobalTracer.Instance, NoopTracerFactory.Create());
        }

        private void ClearProperty(string name)
        {
            Environment.SetEnvironmentVariable(name, null);
        }

        private void SetProperty(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        }

        [Fact]
        public void TestFromEnv()
        {
            SetProperty(Configuration.JaegerServiceName, "Test");
            Assert.NotNull(Configuration.FromEnv(_loggerFactory).GetTracer());
            Assert.False(GlobalTracer.IsRegistered());
        }

        [Fact]
        public void TestFromIConfig()
        {
            var arrayDict = new Dictionary<string, string>
            {
                {Configuration.JaegerServiceName, "Test"},
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(arrayDict)
                .Build();

            Assert.NotNull(Configuration.FromIConfiguration(_loggerFactory, configuration).GetTracer());
            Assert.False(GlobalTracer.IsRegistered());
        }

        [Fact]
        public void TestSenderWithTargetFromEnv()
        {
            SetProperty(Configuration.JaegerGrpcTarget, "jaeger-collector:14250");
            ISender sender = Configuration.SenderConfiguration.FromEnv(_loggerFactory).GetSender();
            Assert.IsType<GrpcSender>(sender);
        }
    }
}
