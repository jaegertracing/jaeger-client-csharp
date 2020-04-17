using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using Jaeger.Reporters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenTracing.Noop;
using OpenTracing.Util;
using Xunit;

namespace Jaeger.Senders.Thrift.Tests
{
    public class ConfigurationTests : IDisposable
    {
        private const string TestProperty = "TestProperty";

        private readonly ILoggerFactory _loggerFactory;

        public ConfigurationTests()
        {
            _loggerFactory = NullLoggerFactory.Instance;
            Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(_loggerFactory)
                .RegisterSenderFactory<ThriftSenderFactory>();

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
        public void TestConfigurationWithDefaultReporterReturnsUdpClientRemoteReporter()
        {
            SetProperty(Configuration.JaegerServiceName, "Test");
            Tracer tracer = (Tracer)Configuration.FromEnv(_loggerFactory).GetTracer();
            Assert.IsType<RemoteReporter>(tracer.Reporter);
            Assert.Contains("UdpSender", tracer.Reporter.ToString());
        }

        [Fact]
        public void TestSenderWithEndpointWithoutAuthData()
        {
            SetProperty(Configuration.JaegerEndpoint, "https://jaeger-collector:14268/api/traces");
            ISender sender = Configuration.SenderConfiguration.FromEnv(_loggerFactory).GetSender();
            Assert.True(sender is HttpSender);
        }

        [Fact]
        public void TestSenderWithAgentDataFromEnv()
        {
            SetProperty(Configuration.JaegerAgentHost, "jaeger-agent");
            SetProperty(Configuration.JaegerAgentPort, "6832");
            Assert.Throws<SocketException>(() => Configuration.SenderConfiguration.FromEnv(_loggerFactory).GetSender());
            //ISender sender = Configuration.SenderConfiguration.FromEnv(_loggerFactory).GetSender();
            //Assert.True(sender is UdpSender);
        }

        [Fact]
        public void TestCustomSender()
        {
            String endpoint = "https://custom-sender-endpoint:14268/api/traces";
            SetProperty(Configuration.JaegerEndpoint, "https://jaeger-collector:14268/api/traces");
            CustomSender customSender = new CustomSender(endpoint);
            Configuration.SenderConfiguration senderConfiguration = new Configuration.SenderConfiguration(_loggerFactory)
                .WithSender(customSender);
            Assert.Equal(endpoint, ((CustomSender)senderConfiguration.GetSender()).Endpoint);
        }

       [Fact]
        public void TestSenderWithBasicAuthUsesHttpSender()
        {
            Configuration.SenderConfiguration senderConfiguration = new Configuration.SenderConfiguration(_loggerFactory)
                    .WithEndpoint("https://jaeger-collector:14268/api/traces")
                    .WithAuthUsername("username")
                    .WithAuthPassword("password");
            Assert.True(senderConfiguration.GetSender() is HttpSender);
        }

       [Fact]
        public void TestSenderWithAuthTokenUsesHttpSender()
        {
            Configuration.SenderConfiguration senderConfiguration = new Configuration.SenderConfiguration(_loggerFactory)
                    .WithEndpoint("https://jaeger-collector:14268/api/traces")
                    .WithAuthToken("authToken");
            Assert.True(senderConfiguration.GetSender() is HttpSender);
        }

        [Fact]
        public void TestSenderWithNoPropertiesReturnsUdpSender()
        {
            Assert.True(Configuration.SenderConfiguration.FromEnv(_loggerFactory).GetSender() is UdpSender);
        }

        [Fact]
        public void TestSenderWithThriftSelectedOnDefaultResolverReturnsThriftSender()
        {
            SetProperty(Configuration.JaegerSenderFactory, "thrift");

            Assert.True(Configuration.SenderConfiguration.FromEnv(_loggerFactory).GetSender() is ThriftSender);
        }

        [Fact]
        public void TestSenderWithThriftSelectedOnEmptyResolverReturnsNoopSender()
        {
            SetProperty(Configuration.JaegerSenderFactory, "thrift");

            Assert.True(Configuration.SenderConfiguration.FromEnv(_loggerFactory)
                .WithSenderResolver(new SenderResolver(_loggerFactory))
                .GetSender() is NoopSender);
        }

        [Fact]
        public void TestSenderWithAllPropertiesReturnsHttpSender()
        {
            SetProperty(Configuration.JaegerEndpoint, "https://jaeger-collector:14268/api/traces");
            SetProperty(Configuration.JaegerAgentHost, "jaeger-agent");
            SetProperty(Configuration.JaegerAgentPort, "6832");

            Assert.True(Configuration.SenderConfiguration.FromEnv(_loggerFactory).GetSender() is HttpSender);
        }

        private class CustomSender : HttpSender
        {
            public string Endpoint { get; }

            public CustomSender(string endpoint)
                : base(endpoint)
            {
                Endpoint = endpoint;
            }
        }
    }
}
