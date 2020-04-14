using System;
using Jaeger.Util;
using NSubstitute;
using Xunit;

namespace Jaeger.Core.Tests.Util
{
    public class RateLimiterTests
    {
        [Fact]
        public void RateLimiter()
        {
            var clock = Substitute.For<IClock>();
            var time = DateTime.UtcNow;

            clock.UtcNow().Returns(x => time);

            var rateLimiter = new RateLimiter(2.0, 2.0, clock);

            Assert.True(rateLimiter.CheckCredit(1.0));
            Assert.True(rateLimiter.CheckCredit(1.0));
            Assert.False(rateLimiter.CheckCredit(1.0));

            // move time 250ms forward, not enough credits to pay for 1.0 item
            time = time.AddMilliseconds(250);
            Assert.False(rateLimiter.CheckCredit(1.0));

            // move time 500ms forward, now enough credits to pay for 1.0 item
            time = time.AddMilliseconds(500);
            Assert.True(rateLimiter.CheckCredit(1.0));
            Assert.False(rateLimiter.CheckCredit(1.0));

            // move time 5s forward, enough to accumulate credits for 10 messages, but it should still be capped at 2
            time = time.AddSeconds(5);
            Assert.True(rateLimiter.CheckCredit(1.0));
            Assert.True(rateLimiter.CheckCredit(1.0));
            Assert.False(rateLimiter.CheckCredit(1.0));
            Assert.False(rateLimiter.CheckCredit(1.0));
            Assert.False(rateLimiter.CheckCredit(1.0));
        }
    }
}
