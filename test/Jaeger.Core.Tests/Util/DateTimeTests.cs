using System;
using Jaeger.Util;
using Xunit;

namespace Jaeger.Core.Tests.Util
{
    public class DateTimeTests
    {
        [Fact]
        public void DateTimeMicroseconds()
        {
            var time = new DateTime(1988, 12, 29, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = 599356800000L;
            const long millisecondsPerDay = 86400000L;

            Assert.Equal(time.ToUnixTimeMilliseconds(), timestamp);
            Assert.Equal(time.ToUnixTimeMicroseconds(), timestamp * 1000);

            var past = time.Subtract(TimeSpan.FromDays(60));
            var paststamp = timestamp - (60 * millisecondsPerDay);
            Assert.Equal(past.ToUnixTimeMilliseconds(), paststamp);
            Assert.Equal(past.ToUnixTimeMicroseconds(), paststamp * 1000);

            var future = time.Add(TimeSpan.FromDays(60));
            var futurestamp = timestamp + (60 * millisecondsPerDay);
            Assert.Equal(future.ToUnixTimeMilliseconds(), futurestamp);
            Assert.Equal(future.ToUnixTimeMicroseconds(), futurestamp * 1000);
        }
    }
}
