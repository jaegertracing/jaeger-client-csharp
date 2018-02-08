using System;
using System.Collections.Generic;
using LetsTrace.Util;
using NSubstitute;
using OpenTracing;
using Xunit;

namespace LetsTrace.Tests
{
    public class SpanTests
    {

        [Fact]
        public void Span_Constructor_ShouldTAssignEverythingCorrectlyWhenPassed()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var ref1Context = Substitute.For<ISpanContext>();
            var tags = new Dictionary<string, string> { { "key", "value" } };
            var references = new List<Reference> {
                new Reference("type1", ref1Context)
            };

            var s = new Span(tracer, operationName, spanContext, startTimestamp, tags, references);
            Assert.Equal(operationName, s.OperationName);
            Assert.Equal(spanContext, s.Context);
            Assert.Equal(startTimestamp, s.StartTimestamp);
            Assert.Equal(tags, s.Tags);
            Assert.Equal(references, s.References);
        }

        [Fact]
        public void Span_Constructor_ShouldThrowIfTracerIsNull()
        {
            var spanContext = Substitute.For<ISpanContext>();
            
            var ex = Assert.Throws<ArgumentNullException>(() => new Span(null, "", spanContext));
            Assert.Equal("tracer", ex.ParamName);
        }

        [Fact]
        public void Span_Constructor_ShouldThrowIfOperationNameIsNullOrEmpty()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var spanContext = Substitute.For<ISpanContext>();
            
            var ex1 = Assert.Throws<ArgumentException>(() => new Span(tracer, "", spanContext));
            Assert.Equal("Argument is empty\nParameter name: operationName", ex1.Message);

            var ex2 = Assert.Throws<ArgumentException>(() => new Span(tracer, null, spanContext));
            Assert.Equal("Argument is null\nParameter name: operationName", ex2.Message);
        }

        [Fact]
        public void Span_Constructor_ShouldThrowIfContextIsNull()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            
            var ex = Assert.Throws<ArgumentNullException>(() => new Span(tracer, "testing", null));
            Assert.Equal("context", ex.ParamName);
        }

        [Fact]
        public void Span_Constructor_ShouldDefaultStartTimestampTagsAndReferencesIfNull()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;

            tracer.Clock.Returns(clock);
            clock.CurrentTime().Returns(startTimestamp);

            var s = new Span(tracer, operationName, spanContext, null, null, null);
            
            clock.Received(1).CurrentTime();
            Assert.Equal(operationName, s.OperationName);
            Assert.Equal(spanContext, s.Context);
            Assert.Equal(startTimestamp, s.StartTimestamp);
            Assert.Equal(new Dictionary<string, string>(), s.Tags);
            Assert.Equal(new List<Reference>(), s.References);
        }

        [Fact]
        public void Span_Finish_ShouldReportTheSpanToTheTracerOnce()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var currentTime = DateTimeOffset.Now.AddSeconds(1);

            tracer.Clock.Returns(clock);
            clock.CurrentTime().Returns(currentTime);

            var span = new Span(tracer, "testing", spanContext, startTimestamp);
            span.Finish();
            span.Finish();
            span.Finish();

            clock.Received(3).CurrentTime();
            tracer.Received(1).ReportSpan(Arg.Is<ILetsTraceSpan>(s => s == span));
            Assert.Equal(currentTime, span.FinishTimestamp);
        }

        [Fact]
        public void Span_Finish_WithFinishTimestamp_ShouldReportTheSpanToTheTracerOnce()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var currentTime = DateTimeOffset.Now.AddSeconds(1);

            tracer.Clock.Returns(clock);

            var span = new Span(tracer, "testing", spanContext, startTimestamp);
            span.Finish(currentTime);
            span.Finish(currentTime);
            span.Finish(currentTime);

            clock.Received(0).CurrentTime();
            tracer.Received(1).ReportSpan(Arg.Is<ILetsTraceSpan>(s => s == span));
            Assert.Equal(currentTime, span.FinishTimestamp);
        }

        [Fact]
        public void Span_Dispose_ShouldReportTheSpanToTheTracerOnce()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var currentTime = DateTimeOffset.Now.AddSeconds(1);

            tracer.Clock.Returns(clock);
            clock.CurrentTime().Returns(currentTime);

            var span = new Span(tracer, "testing", spanContext, startTimestamp);
            span.Dispose();
            span.Dispose();
            span.Dispose();

            clock.Received(3).CurrentTime();
            tracer.Received(1).ReportSpan(Arg.Is<ILetsTraceSpan>(s => s == span));
            Assert.Equal(currentTime, span.FinishTimestamp);
        }

        [Fact]
        public void Span_GetBaggageItem_ShouldUseTheContextBaggage()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var baggage = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

            spanContext.GetBaggageItems().Returns(baggage);

            var span = new Span(tracer, "testing", spanContext, startTimestamp);
            var value = span.GetBaggageItem("key2");

            Assert.Equal(baggage["key2"], value);
        }

        [Fact]
        public void Span_GetBaggageItem_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var baggage = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

            spanContext.GetBaggageItems().Returns(baggage);

            var span = new Span(tracer, "testing", spanContext, startTimestamp);
            var value = span.GetBaggageItem("key3");

            Assert.Null(value);
        }

        [Fact]
        public void Span_SetBaggageItem_ShouldOffLoadToTracer()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var key = "key";
            var value = "value";

            tracer.SetBaggageItem(
                Arg.Is<ILetsTraceSpan>(s => s.OperationName == operationName),
                Arg.Is<string>(k => k == key),
                Arg.Is<string>(v => v == value)
            );

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetBaggageItem(key, value);

            tracer.Received(1).SetBaggageItem(Arg.Any<ILetsTraceSpan>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void Span_Log_Fields_ShouldLogAllFields()
        {
            var fields = new List<KeyValuePair<string, object>> { 
                new KeyValuePair<string, object>("log1", "message1"),
                new KeyValuePair<string, object>("log2", "message2")
            };
            
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var currentTime = DateTimeOffset.Now.AddSeconds(1);

            tracer.Clock.Returns(clock);
            clock.CurrentTime().Returns(currentTime);

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.Log(fields);

            clock.Received(1).CurrentTime();
            Assert.Equal(fields, span.Logs[0].Fields);
            Assert.Equal(currentTime, span.Logs[0].Timestamp);
        }

        [Fact]
        public void Span_Log_Fields_WithTimestamp_ShouldLogAllFields()
        {
            var fields = new List<KeyValuePair<string, object>> { 
                new KeyValuePair<string, object>("log1", "message1"),
                new KeyValuePair<string, object>("log2", "message2")
            };
            
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var logTimestamp = DateTimeOffset.Now.AddMilliseconds(150);

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.Log(logTimestamp, fields);

            Assert.Equal(fields, span.Logs[0].Fields);
            Assert.Equal(logTimestamp, span.Logs[0].Timestamp);
        }

        [Fact]
        public void Span_Log_EventName_ShouldLogEventName()
        {
            var eventName = "event, yo";
            
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var clock = Substitute.For<IClock>();
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var currentTime = DateTimeOffset.Now.AddSeconds(1);

            tracer.Clock.Returns(clock);
            clock.CurrentTime().Returns(currentTime);

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.Log(eventName);

            clock.Received(1).CurrentTime();
            Assert.Equal(eventName, span.Logs[0].Message);
            Assert.Equal(currentTime, span.Logs[0].Timestamp);
        }

        [Fact]
        public void Span_Log_EventName_WithTimestamp_ShouldLogAllFields()
        {
            var eventName = "event, yo";
            
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var logTimestamp = DateTimeOffset.Now.AddMilliseconds(150);

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.Log(logTimestamp, eventName);

            Assert.Equal(eventName, span.Logs[0].Message);
            Assert.Equal(logTimestamp, span.Logs[0].Timestamp);
        }

        [Fact]
        public void Span_SetOperationName()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var logTimestamp = DateTimeOffset.Now.AddMilliseconds(150);

            var span = new Span(tracer, operationName, spanContext, startTimestamp);

            Assert.Equal(operationName, span.OperationName);
            
            var newOperationName = "testing2";
            span.SetOperationName(newOperationName);

            Assert.Equal(newOperationName, span.OperationName);
        }

        [Fact]
        public void Span_SetTag_Bool_ShouldSetTag()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var tagName = "testing.tag";
            var value = true;

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetTag(tagName, value);

            Assert.Equal(value.ToString(), span.Tags[tagName]);
        }

        [Fact]
        public void Span_SetTag_Double_ShouldSetTag()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var tagName = "testing.tag";
            var value = 3D;

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetTag(tagName, value);

            Assert.Equal(value.ToString(), span.Tags[tagName]);
        }

        [Fact]
        public void Span_SetTag_Int_ShouldSetTag()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var tagName = "testing.tag";
            var value = 55;

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetTag(tagName, value);

            Assert.Equal(value.ToString(), span.Tags[tagName]);
        }

        [Fact]
        public void Span_SetTag_String_ShouldSetTag()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var tagName = "testing.tag";
            var value = "testing, yo";

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetTag(tagName, value);

            Assert.Equal(value.ToString(), span.Tags[tagName]);
        }

        [Fact]
        public void Span_SetTag_ShouldOverwriteTag()
        {
            var tracer = Substitute.For<ILetsTraceTracer>();
            var operationName = "testing";
            var spanContext = Substitute.For<ISpanContext>();
            var startTimestamp = DateTimeOffset.Now;
            var tagName = "testing.tag";
            var value = "testing, yo";

            var span = new Span(tracer, operationName, spanContext, startTimestamp);
            span.SetTag(tagName, value);

            Assert.Equal(value.ToString(), span.Tags[tagName]);

            var newValue = "testing 2, yo";
            span.SetTag(tagName, newValue);
        
            Assert.Equal(newValue.ToString(), span.Tags[tagName]);
        }
    }
}