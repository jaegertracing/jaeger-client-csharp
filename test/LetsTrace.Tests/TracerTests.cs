using System;
using LetsTrace.Propagation;
using LetsTrace.Reporters;
using LetsTrace.Util;
using NSubstitute;
using OpenTracing;
using OpenTracing.Propagation;
using Xunit;

namespace LetsTrace.Tests
{
    public class TracerTests
    {
        [Fact]
        public void Tracer_Constructor_ShouldThrowWhenServiceNameIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Tracer(null, null, null));
            Assert.Equal("serviceName", ex.ParamName);
        }

        [Fact]
        public void Tracer_Constructor_ShouldThrowWhenReporterIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new Tracer("testingService", null, null));
            Assert.Equal("reporter", ex.ParamName);
        }

        [Fact]
        public void Tracer_Constructor_ShouldThrowWhenHostIPv4IsNull()
        {
            var reporter = Substitute.For<IReporter>();

            var ex = Assert.Throws<ArgumentNullException>(() => new Tracer("testingService", reporter, null));
            Assert.Equal("hostIPv4", ex.ParamName);
        }

        [Fact]
        public void Tracer_Constructor_ShouldSetupDefaultInjectorsAndExtractors()
        {
            var reporter = Substitute.For<IReporter>();

            var tracer = new Tracer("testingService", reporter, "192.168.1.1");

            Assert.Contains(tracer._injectors, i => i.Key == Formats.TextMap.Name);
            Assert.Contains(tracer._injectors, i => i.Key == Formats.HttpHeaders.Name);
            Assert.Contains(tracer._extractors, i => i.Key == Formats.TextMap.Name);
            Assert.Contains(tracer._extractors, i => i.Key == Formats.HttpHeaders.Name);
        }

        [Fact]
        public void Tracer_BuildSpan_ShouldPassItselfAndOperationNameToSpanBuilder()
        {
            var reporter = Substitute.For<IReporter>();
            var operationName = "testing";

            var tracer = new Tracer("testingService", reporter, "192.168.1.1");
            var span = (ILetsTraceSpan)tracer.BuildSpan(operationName).Start();

            Assert.Equal(operationName, span.OperationName);
            Assert.Equal(tracer, span.Tracer);
        }

        [Fact]
        public void Tracer_ReportSpan_ShouldPassSpanToReporter()
        {
            var reporter = Substitute.For<IReporter>();
            var span = Substitute.For<ILetsTraceSpan>();

            var tracer = new Tracer("testingService", reporter, "192.168.1.1");
            tracer.ReportSpan(span);

            reporter.Received(1).Report(Arg.Any<ILetsTraceSpan>());
        }

        [Fact]
        public void Tracer_ExtractAndInject_ShouldUseTheCorrectCodec()
        {
            var reporter = Substitute.For<IReporter>();
            var injector = Substitute.For<IInjector>();
            var extractor = Substitute.For<IExtractor>();
            var carrier = "carrier, yo";
            var spanContext = Substitute.For<ISpanContext>();

            var format = new Format<string>("format");

            extractor.Extract(Arg.Is<string>(c => c == carrier));
            injector.Inject(Arg.Is<ISpanContext>(sc => sc == spanContext), Arg.Is<string>(c => c == carrier));

            var tracer = new Tracer("testingService", reporter, "192.168.1.1");
            tracer.AddCodec(format.Name, injector, extractor);
            tracer.Extract(format, carrier);
            tracer.Inject(spanContext, format, carrier);

            extractor.Received(1).Extract(Arg.Any<string>());
            injector.Received(1).Inject(Arg.Any<ISpanContext>(), Arg.Any<string>());
        }

        [Fact]
        public void Tracer_ExtractAndInject_ShouldThrowWhenCodecDoesNotExist()
        {
            var reporter = Substitute.For<IReporter>();
            var carrier = "carrier, yo";
            var spanContext = Substitute.For<ISpanContext>();
            var format = new Format<string>("format");

            var tracer = new Tracer("testingService", reporter, "192.168.1.1");
            var ex = Assert.Throws<Exception>(() => tracer.Extract(format, carrier));
            Assert.Equal($"{format.Name} is not a supported extraction format", ex.Message);

            ex = Assert.Throws<Exception>(() => tracer.Inject(spanContext, format, carrier));
            Assert.Equal($"{format.Name} is not a supported injection format", ex.Message);
        }

        [Fact]
        public void Tracer_SetBaggageItem_ShouldAddAndSetBaggageItems()
        {
            var reporter = Substitute.For<IReporter>();
            var spanContext = new SpanContext(new TraceId());

            var tracer = new Tracer("testingService", reporter, "192.168.1.1");
            var span = new Span(tracer, "testing", spanContext);

            var key = "key1";
            var value = "value1";
            var value2 = "value2";

            tracer.SetBaggageItem(span, key, value);
            Assert.Equal(value, span.GetBaggageItem(key));

            tracer.SetBaggageItem(span, key, value2);
            Assert.Equal(value2, span.GetBaggageItem(key));
        }
    }
}
