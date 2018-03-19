using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LetsTrace.Jaeger.Transport.Sender;
using NSubstitute;
using Thrift.Protocols;
using Xunit;
using JaegerSpan = Jaeger.Thrift.Span;
using JaegerProcess = Jaeger.Thrift.Process;

namespace LetsTrace.Jaeger.Tests.Transport.Sender
{
    public class JaegerSenderTests
    {
        private class TestingSender : JaegerSender
        {
            public List<JaegerSpan> Buffer => _buffer;
            public JaegerProcess Process => _process;

            public Func<List<JaegerSpan>, CancellationToken, Task<int>> SendAsyncDelegate; 

            internal TestingSender() : base(new TBinaryProtocol.Factory())
            {}

            protected override Task<int> SendAsync(List<JaegerSpan> spans, CancellationToken cancellationToken)
            {
                return SendAsyncDelegate(spans, cancellationToken);
            }
        }

        [Fact]
        public void BufferItem_ShouldReturnCountAfterAddingItem()
        {
            var sender = new TestingSender();
            var item = new JaegerSpan();

            Assert.Equal(1, sender.BufferItem(item));
            Assert.Equal(2, sender.BufferItem(item));
            Assert.Equal(2, sender.Buffer.Count);
        }

        [Fact]
        public async void FlushAsync_ShouldThrowWhenProcessIsNull()
        {
            var sender = new TestingSender();

            await Assert.ThrowsAsync<ArgumentNullException>(() => sender.FlushAsync(null, CancellationToken.None));
        }

        [Fact]
        public async void FlushAsync_ShouldCallSendAsyncAndReturnCountOfSpansSent()
        {
            var sender = new TestingSender
            {
                SendAsyncDelegate = Substitute.For<Func<List<JaegerSpan>, CancellationToken, Task<int>>>()
            };
            var item = new JaegerSpan();
            var process = new JaegerProcess("testingService");
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            sender.BufferItem(item);
            sender.BufferItem(item);
            sender.BufferItem(item);
            sender.BufferItem(item);

            sender.SendAsyncDelegate(Arg.Is<List<JaegerSpan>>(js => js.Count == 4), Arg.Is(token)).Returns(4);

            var sent = await sender.FlushAsync(process, token);

            Assert.Equal(4, sent);
            Assert.Empty(sender.Buffer);
            await sender.SendAsyncDelegate.Received(1)(Arg.Is<List<JaegerSpan>>(js => js.Count == 4), Arg.Is(token));
        }

        [Fact]
        public async void FlushAsync_ShouldNotCallSendAsyncWhenTheBufferIsEmpty()
        {
            var sender = new TestingSender
            {
                SendAsyncDelegate = Substitute.For<Func<List<JaegerSpan>, CancellationToken, Task<int>>>()
            };
            var process = new JaegerProcess("testingService");
            var cts = new CancellationTokenSource();
            var token = cts.Token;


            var sent = await sender.FlushAsync(process, token);

            Assert.Equal(0, sent);
            Assert.Empty(sender.Buffer);
            await sender.SendAsyncDelegate.Received(0)(Arg.Any<List<JaegerSpan>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async void Dispose_ShouldFlushAnyItemsCurrentlyInTheBuffer()
        {
            var sender = new TestingSender
            {
                SendAsyncDelegate = Substitute.For<Func<List<JaegerSpan>, CancellationToken, Task<int>>>()
            };
            var item = new JaegerSpan();

            sender.BufferItem(item);
            sender.BufferItem(item);
            sender.BufferItem(item);

            sender.Dispose();

            Assert.Empty(sender.Buffer);
            await sender.SendAsyncDelegate.Received(1)(Arg.Is<List<JaegerSpan>>(js => js.Count == 3), Arg.Any<CancellationToken>());
        }
    }
}
