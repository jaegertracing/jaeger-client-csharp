using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Jaeger.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jaeger.Senders.Grpc.Tests
{
    public class GrpcSenderTests
    {
        private const string ServiceName = "TestService";
        private const string OperationName = "TestOperation";

        private readonly ILoggerFactory _loggerFactory;

        public GrpcSenderTests()
        {
            _loggerFactory = NullLoggerFactory.Instance;
        }

        private Span GetSpan()
        {
            // Get dummy tracer, not related to our sender:
            var tracer = new Configuration(ServiceName, _loggerFactory)
                .GetTracer();
            return tracer.BuildSpan(OperationName)
                .Start() as Span;
        }

        [Fact]
        public async Task TestDefaultSenderAppendNoSend()
        {
            var sender = new GrpcSender();
            var span = GetSpan();

            // Only appended, no sending, so this should not fail.
            var count = await sender.AppendAsync(span, CancellationToken.None);
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task TestNotAppendable()
        {
            var maxPacketSize = 1; // Span has around 50 bytes
            var sender = new GrpcSender(GrpcSender.DefaultCollectorGrpcTarget, ChannelCredentials.Insecure, maxPacketSize);
            var span = GetSpan();

            var ex = await Assert.ThrowsAsync<SenderException>(() => sender.AppendAsync(span, CancellationToken.None));
            Assert.Equal(1, ex.DroppedSpanCount);
            Assert.Contains("too large", ex.Message);
        }

        [Fact]
        public async Task TestFlushing()
        {
            var maxPacketSize = 100; // Span has around 50 bytes
            var sender = new GrpcSender(GrpcSender.DefaultCollectorGrpcTarget, ChannelCredentials.Insecure, maxPacketSize);
            var span = GetSpan();

            // Only appended, no sending, so this should not fail.
            // It's exceeding the max bytes, so it will be flushed on next append.
            var count = await sender.AppendAsync(span, CancellationToken.None);
            Assert.Equal(0, count);

            // As we don't have a gRPC endpoint running, this will fail after timeout.
            // Both spans will be lost (both appends).
            var ex = await Assert.ThrowsAsync<SenderException>(() => sender.AppendAsync(span, CancellationToken.None));
            Assert.Equal(2, ex.DroppedSpanCount);

            // Check that we really only failed because of the timeout.
            var rpcEx = Assert.IsType<RpcException>(ex.InnerException?.InnerException);
            Assert.Equal(StatusCode.Unavailable, rpcEx.StatusCode);
        }
    }
}
