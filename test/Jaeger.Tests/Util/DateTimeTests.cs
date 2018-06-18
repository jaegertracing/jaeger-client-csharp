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
            Assert.Equal(time.ToUnixTimeMilliseconds(), time.ToUnixTimeMicroseconds() / 1000);

            var past = time.Subtract(TimeSpan.FromDays(-60));
            Assert.Equal(past.ToUnixTimeMilliseconds(), past.ToUnixTimeMicroseconds() / 1000);

            var future = time.Subtract(TimeSpan.FromDays(+60));
            Assert.Equal(future.ToUnixTimeMilliseconds(), future.ToUnixTimeMicroseconds() / 1000);
        }
    }
}
