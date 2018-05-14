using System;
using Xunit;

namespace Jaeger.Core.Tests
{
    public class SpanIdTests
    {
        private readonly long _spanIdValue;
        private readonly SpanId _spanId;

        public SpanIdTests()
        {
            _spanIdValue = 42;
            _spanId = new SpanId(_spanIdValue);
        }

        [Fact]
        public void Field_ShouldReturnHexString()
        {
            Assert.Equal("2a", _spanId.ToString());
        }

        [Fact]
        public void Field_ShouldBeCastableToInt64()
        {
            var longValue = (long)_spanId;
            Assert.Equal(longValue, Convert.ToInt64(_spanIdValue));
        }
    }
}
