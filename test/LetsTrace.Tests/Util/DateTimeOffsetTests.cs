using System;
using LetsTrace.Util;
using NSubstitute;
using Xunit;

namespace LetsTrace.Tests.Util
{
    public class DateTimeOffsetTests
    {
        [Fact]
        public void DateTimeOffsetMicroseconds()
        {
            var time = DateTimeOffset.Now;
            Assert.Equal(time.ToUnixTimeMilliseconds() * 1000, time.ToUnixTimeMicroseconds());

            var past = time.Subtract(TimeSpan.FromDays(-60));
            Assert.Equal(past.ToUnixTimeMilliseconds() * 1000, past.ToUnixTimeMicroseconds());

            var future = time.Subtract(TimeSpan.FromDays(+60));
            Assert.Equal(future.ToUnixTimeMilliseconds() * 1000, future.ToUnixTimeMicroseconds());
        }
    }
}
