using System;

namespace Jaeger.Core.Util
{
    public static class DateTimeExtensions
    {
        // ToUnixTimeMilliseconds() only exists on DateTimeOffset so we've copied this functionality from:
        // - https://github.com/dotnet/coreclr/blob/master/src/mscorlib/shared/System/DateTime.cs
        // - https://github.com/dotnet/coreclr/blob/master/src/mscorlib/shared/System/DateTimeOffset.cs

        // Number of days in a non-leap year
        private const int DaysPerYear = 365;
        // Number of days in 4 years
        private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
        // Number of days in 100 years
        private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
        // Number of days in 400 years
        private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097
        // Number of days from 1/1/0001 to 12/31/1969
        private const int DaysTo1970 = DaysPer400Years * 4 + DaysPer100Years * 3 + DaysPer4Years * 17 + DaysPerYear; // 719,162

        private const long UnixEpochTicks = DaysTo1970 * TimeSpan.TicksPerDay;
        private const long UnixEpochMilliseconds = UnixEpochTicks / TimeSpan.TicksPerMillisecond; // 62,135,596,800,000

        // a microsecond is 1000 milliseconds
        public static long ToUnixTimeMicroseconds(this DateTime utcTimestamp) => utcTimestamp.ToUnixTimeMilliseconds() * 1000;

        public static long ToUnixTimeMilliseconds(this DateTime utcTimestamp)
        {
            // Truncate sub-millisecond precision before offsetting by the Unix Epoch to avoid
            // the last digit being off by one for dates that result in negative Unix times
            long milliseconds = utcTimestamp.Ticks / TimeSpan.TicksPerMillisecond;
            return milliseconds - UnixEpochMilliseconds;
        }
    }
}
