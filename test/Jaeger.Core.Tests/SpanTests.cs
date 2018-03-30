using System;
using System.Collections.Generic;
using Jaeger.Core.Util;
using NSubstitute;
using OpenTracing;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class SpanTests
    {

        [Fact]
        public void Span_Constructor_ShouldAssignEverythingCorrectlyWhenPassed()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var ref1Context = Substitute.For<IJaegerCoreSpanContext>();
            var tags = new Dictionary<string, object> { { "key", "something"} };
            var references = new List<Reference> {
                new Reference("type1", ref1Context)
            };

            var s = new Span(tracer, operationName, spanContext, startTimestamp, tags, references);
            Assert.Equal(operationName, s.OperationName);
            Assert.Equal(spanContext, s.Context);
            Assert.Equal(startTimestamp, s.StartTimestampUtc);
            Assert.Equal(tags, s.Tags);
            Assert.Equal(references, s.References);
        }

        [Fact]
        public void Span_Constructor_ShouldThrowIfTracerIsNull()
        {
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();

            var ex = Assert.Throws<ArgumentNullException>(() => new Span(null, "", spanContext));
            Assert.Equal("tracer", ex.ParamName);
        }

        [Fact]
        public void Span_Constructor_ShouldThrowIfOperationNameIsNullOrEmpty()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();

            var ex1 = Assert.Throws<ArgumentException>(() => new Span(tracer, "", spanContext));
            Assert.StartsWith("Argument is empty", ex1.Message);

            var ex2 = Assert.Throws<ArgumentException>(() => new Span(tracer, null, spanContext));
            Assert.StartsWith("Argument is null", ex2.Message);
        }

        [Fact]
        public void Span_Constructor_ShouldThrowIfContextIsNull()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();

            var ex = Assert.Throws<ArgumentNullException>(() => new Span(tracer, "testing", null));
            Assert.Equal("context", ex.ParamName);
        }

        [Fact]
        public void Span_Constructor_ShouldDefaultStartTimestampTagsAndReferencesIfNull()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;

            tracer.Clock.Returns(clock);
            clock.UtcNow().Returns(startTimestamp);

            var s = new Span(tracer, operationName, spanContext, null, null, null);

            clock.Received(1).UtcNow();
            Assert.Equal(operationName, s.OperationName);
            Assert.Equal(spanContext, s.Context);
            Assert.Equal(startTimestamp, s.StartTimestampUtc);
            Assert.Equal(new Dictionary<string, object>(), s.Tags);
            Assert.Equal(new List<Reference>(), s.References);
        }

        [Fact]
        public void Span_Finish_ShouldReportTheSpanToTheTracerOnce()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var currentTime = DateTime.UtcNow.AddSeconds(1);

            tracer.Clock.Returns(clock);
            clock.UtcNow().Returns(currentTime);

            var span = new Span(tracer, "testing", spanContext, startTimestamp);
            span.Finish();
            span.Finish();
            span.Finish();

            clock.Received(3).UtcNow();
            tracer.Received(1).ReportSpan(Arg.Is<IJaegerCoreSpan>(s => s == span));
            Assert.Equal(currentTime, span.FinishTimestampUtc);
        }

        [Fact]
        public void Span_Finish_WithFinishTimestamp_ShouldReportTheSpanToTheTracerOnce()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var currentTime = DateTime.UtcNow.AddSeconds(1);

            tracer.Clock.Returns(clock);

            var span = new Span(tracer, "testing", spanContext, startTimestamp);
            span.Finish(currentTime);
            span.Finish(currentTime);
            span.Finish(currentTime);

            clock.Received(0).UtcNow();
            tracer.Received(1).ReportSpan(Arg.Is<IJaegerCoreSpan>(s => s == span));
            Assert.Equal(currentTime, span.FinishTimestampUtc);
        }

        [Fact]
        public void Span_Dispose_ShouldReportTheSpanToTheTracerOnce()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var currentTime = DateTime.UtcNow.AddSeconds(1);

            tracer.Clock.Returns(clock);
            clock.UtcNow().Returns(currentTime);

            var span = new Span(tracer, "testing", spanContext, startTimestamp);
            span.Dispose();
            span.Dispose();
            span.Dispose();

            clock.Received(3).UtcNow();
            tracer.Received(1).ReportSpan(Arg.Is<IJaegerCoreSpan>(s => s == span));
            Assert.Equal(currentTime, span.FinishTimestampUtc);
        }

        [Fact]
        public void Span_GetBaggageItem_ShouldUseTheContextBaggage()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var baggage = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

            spanContext.GetBaggageItems().Returns(baggage);

            var span = new Span(tracer, "testing", spanContext, startTimestamp);
            var value = span.GetBaggageItem("key2");

            Assert.Equal(baggage["key2"], value);
        }

        [Fact]
        public void Span_GetBaggageItem_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var baggage = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

            spanContext.GetBaggageItems().Returns(baggage);

            var span = new Span(tracer, "testing", spanContext, startTimestamp);
            var value = span.GetBaggageItem("key3");

            Assert.Null(value);
        }

        [Fact]
        public void Span_SetBaggageItem_ShouldOffLoadToTracer()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var key = "key";
            var value = "value";

            tracer.SetBaggageItem(
                Arg.Is<IJaegerCoreSpan>(s => s.OperationName == operationName),
                Arg.Is<string>(k => k == key),
                Arg.Is<string>(v => v == value)
            );

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetBaggageItem(key, value);

            tracer.Received(1).SetBaggageItem(Arg.Any<IJaegerCoreSpan>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void Span_Log_Fields_ShouldLogAllFields()
        {
            var fields = new Dictionary<string, object> {
                { "log1", "message1" },
                { "log2", false },
                { "log3", new Dictionary<string, string> { { "key", "value" } } },
                { "log4", new Clock() }
            };

            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var currentTime = DateTime.UtcNow.AddSeconds(1);

            tracer.Clock.Returns(clock);
            clock.UtcNow().Returns(currentTime);

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.Log(fields);

            clock.Received(1).UtcNow();
            Assert.True(span.Logs[0].Fields["log1"] is string);
            Assert.True(span.Logs[0].Fields["log2"] is bool);
            Assert.True(span.Logs[0].Fields["log3"] is Dictionary<string, string>); // TODO Where do we check/restrict the types?
            Assert.True(span.Logs[0].Fields["log4"] is Clock); // TODO Where do we check/restrict the types?
            Assert.Equal(currentTime, span.Logs[0].TimestampUtc);
        }

        [Fact]
        public void Span_Log_Fields_WithTimestamp_ShouldLogAllFields()
        {
            var fields = new Dictionary<string, object> {
                { "log1", new byte[] { 0x20, 0x20 } },
                { "log2", 15m }
            };

            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var logTimestamp = DateTime.UtcNow.AddMilliseconds(150);

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.Log(logTimestamp, fields);

            Assert.True(span.Logs[0].Fields["log1"] is byte[]);
            Assert.True(span.Logs[0].Fields["log2"] is decimal);
            Assert.Equal(logTimestamp, span.Logs[0].TimestampUtc);
        }

        [Fact]
        public void Span_Log_EventName_ShouldLogEventName()
        {
            var eventName = "event, yo";

            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var currentTime = DateTime.UtcNow.AddSeconds(1);

            tracer.Clock.Returns(clock);
            clock.UtcNow().Returns(currentTime);

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.Log(eventName);

            clock.Received(1).UtcNow();
            Assert.Equal(eventName, span.Logs[0].Fields[LogFields.Event]);
            Assert.Equal(currentTime, span.Logs[0].TimestampUtc);
        }

        [Fact]
        public void Span_Log_EventName_WithTimestamp_ShouldLogAllFields()
        {
            var eventName = "event, yo";

            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var logTimestamp = DateTime.UtcNow.AddMilliseconds(150);

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.Log(logTimestamp, eventName);

            Assert.Equal(eventName, span.Logs[0].Fields[LogFields.Event]);
            Assert.Equal(logTimestamp, span.Logs[0].TimestampUtc);
        }

        [Fact]
        public void Span_SetOperationName()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var logTimestamp = DateTime.UtcNow.AddMilliseconds(150);

            var span = new Span(tracer, operationName, spanContext, startTimestamp);

            Assert.Equal(operationName, span.OperationName);

            var newOperationName = "testing2";
            span.SetOperationName(newOperationName);

            Assert.Equal(newOperationName, span.OperationName);
        }

        [Fact]
        public void Span_SetTag_Bool_ShouldSetTag()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var tagName = "testing.tag";
            var value = true;

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetTag(tagName, value);

            Assert.Equal(value, span.Tags[tagName]);
        }

        [Fact]
        public void Span_SetTag_Double_ShouldSetTag()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var tagName = "testing.tag";
            var value = 3D;

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetTag(tagName, value);

            Assert.Equal(value, span.Tags[tagName]);
        }

        [Fact]
        public void Span_SetTag_Int_ShouldSetTag()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var tagName = "testing.tag";
            var value = 55;

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetTag(tagName, value);

            Assert.Equal(value, span.Tags[tagName]);
        }

        [Fact]
        public void Span_SetTag_String_ShouldSetTag()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var tagName = "testing.tag";
            var value = "testing, yo";

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetTag(tagName, value);

            Assert.Equal(value, span.Tags[tagName]);
        }

        [Fact]
        public void Span_SetTag_ShouldOverwriteTag()
        {
            var tracer = Substitute.For<IJaegerCoreTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<IJaegerCoreSpanContext>();
            var startTimestamp = DateTime.UtcNow;
            var tagName = "testing.tag";
            var value = "testing, yo";

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetTag(tagName, value);

            Assert.Equal(value, span.Tags[tagName]);

            var newValue = 56;
            span.SetTag(tagName, newValue);

            Assert.Equal(newValue, span.Tags[tagName]);
        }
    }
}