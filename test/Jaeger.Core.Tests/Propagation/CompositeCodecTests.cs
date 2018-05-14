using System.Collections.Generic;
using Jaeger.Core.Propagation;
using Jaeger.Core.Reporters;
using NSubstitute;
using OpenTracing.Propagation;
using Xunit;

namespace Jaeger.Core.Tests.Propagation
{
    public class CompositeCodecTests
    {
        private readonly SpanContext _spanContext;
        private readonly ITextMap _mockCarrier;
        private readonly Codec<ITextMap> _mockCodec1;
        private readonly Codec<ITextMap> _mockCodec2;
        private readonly CompositeCodec<ITextMap> _composite;

        public CompositeCodecTests()
        {
            var tracer = new Tracer.Builder("service")
                .WithReporter(new NoopReporter())
                .Build();

            Span span = (Span)tracer.BuildSpan("foo").Start();

            _spanContext = span.Context;

            _mockCarrier = Substitute.For<ITextMap>();
            _mockCodec1 = Substitute.For<Codec<ITextMap>>();
            _mockCodec2 = Substitute.For<Codec<ITextMap>>();
            _composite = new CompositeCodec<ITextMap>(new List<Codec<ITextMap>> { _mockCodec1, _mockCodec2 });
        }

        [Fact]
        public void TestInject()
        {
            _composite.Inject(_spanContext, _mockCarrier);
            _mockCodec1.Received(1).Inject(_spanContext, _mockCarrier);
            _mockCodec2.Received(1).Inject(_spanContext, _mockCarrier);
        }

        [Fact]
        public void TestExtractFromFirstCodec()
        {
            _mockCodec1.Extract(_mockCarrier).Returns(_spanContext);
            Assert.Equal(_spanContext, _composite.Extract(_mockCarrier));
            _mockCodec1.Received(1).Extract(_mockCarrier);
            _mockCodec2.DidNotReceiveWithAnyArgs().Extract(null);
        }

        [Fact]
        public void TestExtractFromSecondCodec()
        {
            _mockCodec2.Extract(_mockCarrier).Returns(_spanContext);
            Assert.Equal(_spanContext, _composite.Extract(_mockCarrier));
            _mockCodec1.Received(1).Extract(_mockCarrier);
            _mockCodec2.Received(1).Extract(_mockCarrier);
        }

        [Fact]
        public void TestExtractFromNoCodec()
        {
            Assert.Null(_composite.Extract(_mockCarrier));
            _mockCodec1.Received(1).Extract(_mockCarrier);
            _mockCodec2.Received(1).Extract(_mockCarrier);
        }

        [Fact]
        public void TestToString()
        {
            var codec1 = new NamedNoopCodec("codec1");
            var codec2 = new NamedNoopCodec("codec2");
            var composite = new CompositeCodec<ITextMap>(new List<Codec<ITextMap>> { codec1, codec2 });
            Assert.Equal("codec1 : codec2", composite.ToString());
        }

        private class NamedNoopCodec : Codec<ITextMap>
        {
            private readonly string _name;

            public NamedNoopCodec(string name)
            {
                _name = name;
            }

            public override string ToString() => _name;

            protected override SpanContext Extract(ITextMap carrier) => null;

            protected override void Inject(SpanContext spanContext, ITextMap carrier)
            {
            }
        }
    }
}
