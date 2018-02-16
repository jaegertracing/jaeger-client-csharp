using System;
using System.Collections.Generic;
using LetsTrace;
using LetsTrace.Transport.Zipkin.ZipkinJSON;
using NSubstitute;
using OpenTracing;
using Xunit;

namespace LetsTrace.Tests
{
    public class ZipkinJsonTransportTests
    {
        [Fact]
        public void ZipkinJSONTransport_ConvertSpan()
        {
            var span = Substitute.For<ILetsTraceSpan>();
            var traceId = new TraceId { High = 4, Low = 197376 };
            var parentId = new SpanId(1);
            var spanId = new SpanId(2);
            var spanContext = new SpanContext(traceId, spanId, parentId);
            var logs = new List<LogRecord> {
                new LogRecord(DateTimeOffset.Now, new List<Field> { new Field<string> { Key = "key", Value = "message, yo" } })
            };
            var tags = new Dictionary<string, string> {
                { Tags.SpanKind, Tags.SpanKindServer },
                { "randomkey", "randomvalue" }
            };
            var operationName = "testing";
            var startTime = DateTimeOffset.Parse("2/12/2018 5:49:19 PM +00:00");
            var finishTime = DateTimeOffset.Parse("2/12/2018 5:49:37 PM +00:00");
            var serviceName = "testingService";
            var hostIpv4 = "192.168.1.1";
            var tracer = Substitute.For<ILetsTraceTracer>();

            span.Context.Returns(spanContext);
            span.Tracer.Returns(tracer);
            tracer.ServiceName.Returns(serviceName);
            tracer.HostIPv4.Returns(hostIpv4);
            span.Logs.Returns(logs);
            span.Tags.Returns(tags);
            span.OperationName.Returns(operationName);
            span.StartTimestamp.Returns(startTime);
            span.FinishTimestamp.Returns(finishTime);


            var convertedSpan = ZipkinJSONTransport.ConvertSpan(span);

            Assert.Equal(traceId.ToString(), convertedSpan.TraceId);
            Assert.Equal(parentId.ToString(), convertedSpan.ParentId);
            Assert.Equal(spanId.ToString(), convertedSpan.Id);
            Assert.Equal(operationName, convertedSpan.Name);
            Assert.Equal(1518457759000000, convertedSpan.Timestamp);
            Assert.Equal(18000000, convertedSpan.Duration);
            Assert.Equal(serviceName, convertedSpan.LocalEndpoint.ServiceName);
            Assert.Equal(hostIpv4, convertedSpan.LocalEndpoint.Ipv4);
            Assert.Equal("message, yo", convertedSpan.Annotations[0].Value);
            Assert.Equal("randomvalue", convertedSpan.Tags["randomkey"]);
            Assert.Equal(1, convertedSpan.Tags.Count);
            Assert.Equal("SERVER", convertedSpan.Kind.GetValueOrDefault().ToString());
        }
    }
}
