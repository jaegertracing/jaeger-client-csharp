using System;
using Jaeger.Util;
using Xunit;

namespace Jaeger.Tests.Util
{
    public class DateTimeTests
    {
        [Fact]
        public void DateTimeMicroseconds()
        {
            var time = DateTime.UtcNow;
            Assert.Equal(time.ToUnixTimeMilliseconds() * 1000, time.ToUnixTimeMicroseconds());

            var past = time.Subtract(TimeSpan.FromDays(-60));
            Assert.Equal(past.ToUnixTimeMilliseconds() * 1000, past.ToUnixTimeMicroseconds());

            var future = time.Subtract(TimeSpan.FromDays(+60));
            Assert.Equal(future.ToUnixTimeMilliseconds() * 1000, future.ToUnixTimeMicroseconds());
        }
    }
}
