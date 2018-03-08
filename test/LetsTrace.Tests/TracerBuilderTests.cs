using System;
using LetsTrace.Propagation;
using LetsTrace.Reporters;
using LetsTrace.Samplers;
using NSubstitute;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;
using Xunit;

namespace LetsTrace.Tests
{
    public class TracerBuilderTests
    {
        [Fact]
        public void Builder_Constructor_ShouldThrowWhenServiceNameIsNull()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Tracer.Builder(null).Build());
            Assert.Equal("serviceName", ex.ParamName);
        }

        [Fact]
        public void Builder_ShouldUseOpenTracingScopeManagerWhenScopeManagerIsNull()
        {
            var reporter = Substitute.For<IReporter>();
            var sampler = Substitute.For<ISampler>();

            var tracer = new Tracer.Builder("testingService")
                .WithReporter(reporter)
                .WithSampler(sampler)
                .Build();

            Assert.True(tracer.ScopeManager is AsyncLocalScopeManager);
        }

        [Fact]
        public void Builder_ShouldSetupDefaultInjectorsAndExtractors()
        {
            var reporter = Substitute.For<IReporter>();
            var sampler = Substitute.For<ISampler>();
            var scopeManager = Substitute.For<IScopeManager>();

            var tracer = new Tracer.Builder("testingService")
                .WithReporter(reporter)
                .WithSampler(sampler)
                .WithScopeManager(scopeManager)
                .Build();

            Assert.IsType<TextMapPropagationRegistry>(tracer.PropagationRegistry);

            var propagationRegistry = (TextMapPropagationRegistry)tracer.PropagationRegistry;
            Assert.Contains(propagationRegistry._injectors, i => i.Key == BuiltinFormats.TextMap.ToString());
            Assert.Contains(propagationRegistry._injectors, i => i.Key == BuiltinFormats.HttpHeaders.ToString());
            Assert.Contains(propagationRegistry._extractors, i => i.Key == BuiltinFormats.TextMap.ToString());
            Assert.Contains(propagationRegistry._extractors, i => i.Key == BuiltinFormats.HttpHeaders.ToString());
        }

        [Fact]
        public void Builder_ShouldUsePassedInPropagationRegistry()
        {
            var reporter = Substitute.For<IReporter>();
            var sampler = Substitute.For<ISampler>();
            var scopeManager = Substitute.For<IScopeManager>();
            var pReg = Substitute.For<IPropagationRegistry>();

            IFormat<string> format = new Builtin<string>("format");
            var carrier = "carrier, yo";
            pReg.Extract(Arg.Is<IFormat<string>>(f => f == format), Arg.Is<string>(c => c == carrier));
            var spanContext = Substitute.For<ILetsTraceSpanContext>();
            pReg.Inject(Arg.Is<ISpanContext>(sc => sc == spanContext), Arg.Is<IFormat<string>>(f => f == format), Arg.Is<string>(c => c == carrier));

            var tracer = new Tracer.Builder("testingService")
                .WithReporter(reporter)
                .WithSampler(sampler)
                .WithScopeManager(scopeManager)
                .WithPropagationRegistry(pReg)
                .Build();

            tracer.Extract(format, carrier);
            tracer.Inject(spanContext, format, carrier);

            pReg.Received(1).Extract(Arg.Any<IFormat<string>>(), Arg.Any<string>());
            pReg.Received(1).Inject(Arg.Any<ISpanContext>(), Arg.Any<IFormat<string>>(), Arg.Any<string>());
        }
    }
}
