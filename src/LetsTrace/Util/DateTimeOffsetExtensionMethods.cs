using System;

namespace LetsTrace.Util
{
    public static class DateTimeOffsetExtensionMethods
    {
        // a microsecond is 1000 milliseconds
        public static long ToUnixTimeMicroseconds(this DateTimeOffset dto) => dto.ToUnixTimeMilliseconds() * 1000;
    }
}
