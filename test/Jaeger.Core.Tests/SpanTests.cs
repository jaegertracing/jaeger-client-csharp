using System;
using System.Collections.Generic;
using Jaeger.Core.Baggage;
using Jaeger.Core.Metrics;
using Jaeger.Core.Reporters;
using Jaeger.Core.Samplers;
using Jaeger.Core.Util;
using NSubstitute;
using OpenTracing;
using OpenTracing.Tag;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class SpanTests
    {
        private IClock clock;
        private InMemoryReporter reporter;
        private Tracer tracer;
        private Span span;
        private InMemoryMetricsFactory metricsFactory;
        private IMetrics metrics;

        public SpanTests()
        {
            metricsFactory = new InMemoryMetricsFactory();
            reporter = new InMemoryReporter();
            clock = Substitute.For<IClock>();
            metrics = new MetricsImpl(metricsFactory);
            tracer = new Tracer.Builder("SamplerTest")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(true))
                .WithMetrics(metrics)
                .WithClock(clock)
                .WithBaggageRestrictionManager(new DefaultBaggageRestrictionManager())
                .WithExpandExceptionLogs()
                .Build();
            span = (Span)tracer.BuildSpan("some-operation").Start();
        }

        [Fact]
        public void TestSpanMetrics()
        {
            Assert.Equal(1, metricsFactory.GetCounter("jaeger:started_spans", "sampled=y"));
            Assert.Equal(1, metricsFactory.GetCounter("jaeger:traces", "sampled=y,state=started"));
        }

        [Fact]
        public void TestSetAndGetBaggageItem()
        {
            string service = "SamplerTest";
            IBaggageRestrictionManager mgr = Substitute.ForPartsOf<DefaultBaggageRestrictionManager>();
            tracer = new Tracer.Builder(service)
                    .WithReporter(reporter)
                    .WithSampler(new ConstSampler(true))
                    .WithClock(clock)
                    .WithBaggageRestrictionManager(mgr)
                    .Build();
            span = (Span)tracer.BuildSpan("some-operation").Start();

            string key = "key";
            string value = "value";
            mgr.GetRestriction(service, key).Returns(new Restriction(true, 10));
            span.SetBaggageItem(key, "value");
            mgr.Received(1).GetRestriction(service, key);
            Assert.Equal(value, span.GetBaggageItem(key));
        }

        [Fact]
        public void TestSetBooleanTag()
        {
            bool expected = true;
            string key = "tag.key";

            span.SetTag(key, expected);
            Assert.Equal(expected, span.GetTags()[key]);
        }

        [Fact]
        public void TestSetOperationName()
        {
            string expected = "modified.operation";

            Assert.Equal("some-operation", span.OperationName);
            span.SetOperationName(expected);
            Assert.Equal(expected, span.OperationName);
        }

        [Fact]
        public void TestSetStringTag()
        {
            string expected = "expected.value";
            string key = "tag.key";

            span.SetTag(key, expected);
            Assert.Equal(expected, span.GetTags()[key]);
        }

        [Fact]
        public void TestSetNumberTag()
        {
            int expected = 5;
            string key = "tag.key";

            span.SetTag(key, expected);
            Assert.Equal(expected, span.GetTags()[key]);
        }

        [Fact]
        public void TestWithTimestamp()
        {
            DateTimeOffset start = new DateTimeOffset(2018, 4, 12, 14, 0, 1, TimeSpan.Zero);
            DateTimeOffset finish = new DateTimeOffset(2018, 4, 12, 14, 0, 3, TimeSpan.Zero);

            clock.UtcNow().Returns(_ => throw new InvalidOperationException("UtcNow() called"));

            Span span = (Span)tracer.BuildSpan("test-service-name").WithStartTimestamp(start).Start();
            span.Finish(finish);

            Assert.Single(reporter.GetSpans());
            Assert.Equal(start.UtcDateTime, span.StartTimestampUtc);
            Assert.Equal(finish.UtcDateTime, span.FinishTimestampUtc);
        }

        [Fact]
        public void TestMultipleSpanFinishDoesNotCauseMultipleReportCalls()
        {
            Span span = (Span)tracer.BuildSpan("test-service-name").Start();
            span.Finish();

            Assert.Single(reporter.GetSpans());

            Span reportedSpan = reporter.GetSpans()[0];

            // new finish calls will not affect size of reporter.GetSpans()
            span.Finish();

            Assert.Single(reporter.GetSpans());
            Assert.Equal(reportedSpan, reporter.GetSpans()[0]);
        }

        [Fact]
        public void TestWithoutTimestamps()
        {
            DateTime start = new DateTime(2018, 4, 12, 14, 0, 1, DateTimeKind.Utc);
            DateTime finish = new DateTime(2018, 4, 12, 14, 0, 3, DateTimeKind.Utc);
            clock.UtcNow().Returns(start, finish);

            Span span = (Span)tracer.BuildSpan("test-service-name").Start();
            span.Finish();

            Assert.Single(reporter.GetSpans());
            Assert.Equal(start, span.StartTimestampUtc);
            Assert.Equal(finish, span.FinishTimestampUtc);
        }

        [Fact]
        public void TestSpanToString()
        {
            Span span = (Span)tracer.BuildSpan("test-operation").Start();
            SpanContext expectedContext = span.Context;
            SpanContext actualContext = SpanContext.ContextFromString(span.Context.ContextAsString());

            Assert.Equal(expectedContext.TraceId, actualContext.TraceId);
            Assert.Equal(expectedContext.SpanId, actualContext.SpanId);
            Assert.Equal(expectedContext.ParentId, actualContext.ParentId);
            Assert.Equal(expectedContext.Flags, actualContext.Flags);
        }

        [Fact]
        public void TestOperationName()
        {
            string expectedOperation = "leela";
            Span span = (Span)tracer.BuildSpan(expectedOperation).Start();
            Assert.Equal(expectedOperation, span.OperationName);
        }

        [Fact]
        public void TestLogWithTimestamp()
        {
            DateTime expectedTimestamp = new DateTime(2018, 4, 12, 14, 0, 0, DateTimeKind.Utc);
            string expectedLog = "some-log";
            string expectedEvent = "event";
            var expectedFields = new Dictionary<string, object>()
            {
                { expectedEvent, expectedLog }
            };

            span.Log(expectedTimestamp, expectedEvent);
            span.Log(expectedTimestamp, expectedFields);
            span.Log(expectedTimestamp, (string)null);
            span.Log(expectedTimestamp, (Dictionary<string, object>)null);

            LogData actualLogData = span.GetLogs()[0];

            Assert.Equal(expectedTimestamp, actualLogData.TimestampUtc);
            Assert.Equal(expectedEvent, actualLogData.Message);

            actualLogData = span.GetLogs()[1];

            Assert.Equal(expectedTimestamp, actualLogData.TimestampUtc);
            Assert.Null(actualLogData.Message);
            Assert.Equal(expectedFields, actualLogData.Fields);
        }

        [Fact]
        public void TestLog()
        {
            DateTime expectedTimestamp = new DateTime(2018, 4, 12, 14, 0, 0, DateTimeKind.Utc);
            string expectedLog = "some-log";
            string expectedEvent = "expectedEvent";

            clock.UtcNow().Returns(expectedTimestamp);

            span.Log(expectedEvent);

            var expectedFields = new Dictionary<string, object>()
            {
                { expectedEvent, expectedLog }
            };
            span.Log(expectedFields);
            span.Log((string)null);
            span.Log((Dictionary<string, object>)null);

            LogData actualLogData = span.GetLogs()[0];

            Assert.Equal(expectedTimestamp, actualLogData.TimestampUtc);
            Assert.Equal(expectedEvent, actualLogData.Message);

            actualLogData = span.GetLogs()[1];

            Assert.Equal(expectedTimestamp, actualLogData.TimestampUtc);
            Assert.Null(actualLogData.Message);
            Assert.Equal(expectedFields, actualLogData.Fields);
        }

        [Fact]
        public void TestSpanDetectsSamplingPriorityGreaterThanZero()
        {
            Span span = (Span)tracer.BuildSpan("test-service-operation").Start();
            Tags.SamplingPriority.Set(span, 1);

            Assert.Equal(SpanContextFlags.Sampled, span.Context.Flags & SpanContextFlags.Sampled);
            Assert.Equal(SpanContextFlags.Debug, span.Context.Flags & SpanContextFlags.Debug);
        }

        [Fact]
        public void TestSpanDetectsSamplingPriorityLessThanZero()
        {
            Span span = (Span)tracer.BuildSpan("test-service-operation").Start();

            Assert.Equal(SpanContextFlags.Sampled, span.Context.Flags & SpanContextFlags.Sampled);
            Tags.SamplingPriority.Set(span, -1);
            Assert.False(span.Context.Flags.HasFlag(SpanContextFlags.Sampled));
        }

        [Fact]
        public void TestBaggageOneReference()
        {
            ISpan parent = tracer.BuildSpan("foo").Start();
            parent.SetBaggageItem("foo", "bar");

            ISpan child = tracer.BuildSpan("foo")
                .AsChildOf(parent)
                .Start();

            child.SetBaggageItem("a", "a");

            Assert.Null(parent.GetBaggageItem("a"));
            Assert.Equal("a", child.GetBaggageItem("a"));
            Assert.Equal("bar", child.GetBaggageItem("foo"));
        }

        [Fact]
        public void TestBaggageMultipleReferences()
        {
            ISpan parent1 = tracer.BuildSpan("foo").Start();
            parent1.SetBaggageItem("foo", "bar");
            ISpan parent2 = tracer.BuildSpan("foo").Start();
            parent2.SetBaggageItem("foo2", "bar");

            ISpan child = tracer.BuildSpan("foo")
                .AsChildOf(parent1)
                .AddReference(References.FollowsFrom, parent2.Context)
                .Start();

            child.SetBaggageItem("a", "a");
            child.SetBaggageItem("foo2", "b");

            Assert.Null(parent1.GetBaggageItem("a"));
            Assert.Null(parent2.GetBaggageItem("a"));
            Assert.Equal("a", child.GetBaggageItem("a"));
            Assert.Equal("bar", child.GetBaggageItem("foo"));
            Assert.Equal("b", child.GetBaggageItem("foo2"));
        }

        [Fact]
        public void TestImmutableBaggage()
        {
            ISpan span = tracer.BuildSpan("foo").Start();
            span.SetBaggageItem("foo", "bar");
            Assert.Single(span.Context.GetBaggageItems());

            span.SetBaggageItem("foo", null);
            Assert.Empty(span.Context.GetBaggageItems());
        }

        [Fact]
        public void TestExpandExceptionLogs()
        {
            Exception ex = new Exception("foo");
            var logs = new Dictionary<string, object>();
            logs[LogFields.ErrorObject] = ex;
            Span span = (Span)tracer.BuildSpan("foo").Start();
            span.Log(logs);

            var logData = span.GetLogs();
            Assert.Single(logData);
            Assert.Equal(4, logData[0].Fields.Count);

            Assert.Equal(ex, logData[0].Fields[LogFields.ErrorObject]);
            Assert.Equal(ex.Message, logData[0].Fields[LogFields.Message]);
            Assert.Equal(ex.GetType().FullName, logData[0].Fields[LogFields.ErrorKind]);
            Assert.Equal(ex.StackTrace, logData[0].Fields[LogFields.Stack]);
        }

        [Fact]
        public void TestExpandExceptionLogsExpanded()
        {
            Exception ex = new Exception("foo");
            var logs = new Dictionary<string, object>();
            logs[LogFields.ErrorObject] = ex;
            logs[LogFields.Message] = ex.Message;
            logs[LogFields.ErrorKind] = ex.GetType().FullName;
            logs[LogFields.Stack] = ex.StackTrace;
            Span span = (Span)tracer.BuildSpan("foo").Start();
            span.Log(logs);

            var logData = span.GetLogs();
            Assert.Single(logData);
            Assert.Equal(4, logData[0].Fields.Count);

            Assert.Equal(ex, logData[0].Fields[LogFields.ErrorObject]);
            Assert.Equal(ex.Message, logData[0].Fields[LogFields.Message]);
            Assert.Equal(ex.GetType().FullName, logData[0].Fields[LogFields.ErrorKind]);
            Assert.Equal(ex.StackTrace, logData[0].Fields[LogFields.Stack]);
        }

        [Fact]
        public void TestExpandExceptionLogsLoggedNoException()
        {
            Span span = (Span)tracer.BuildSpan("foo").Start();

            object obj = new object();
            var logs = new Dictionary<string, object>();
            logs[LogFields.ErrorObject] = obj;
            span.Log(logs);

            var logData = span.GetLogs();
            Assert.Single(logData);
            Assert.Single(logData[0].Fields);
            Assert.Equal(obj, logData[0].Fields[LogFields.ErrorObject]);
        }

        [Fact]
        public void TestNoExpandExceptionLogs()
        {
            Tracer tracer = new Tracer.Builder("fo")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(true))
                .Build();

            Span span = (Span)tracer.BuildSpan("foo").Start();

            Exception ex = new Exception("ex");
            var logs = new Dictionary<string, object>();
            logs[LogFields.ErrorObject] = ex;
            span.Log(logs);

            var logData = span.GetLogs();
            Assert.Single(logData);
            Assert.Single(logData[0].Fields);
            Assert.Equal(ex, logData[0].Fields[LogFields.ErrorObject]);
        }

        [Fact]
        public void TestSpanNotSampled()
        {
            Tracer tracer = new Tracer.Builder("fo")
                .WithReporter(reporter)
                .WithSampler(new ConstSampler(false))
                .Build();
            ISpan foo = tracer.BuildSpan("foo")
                .Start();
            foo.Log(new Dictionary<string, object>())
                .Finish();
            Assert.Empty(reporter.GetSpans());
        }
    }
}