using System;

namespace LetsTrace.Util
{
    public class Clock : IClock
    {
        public DateTimeOffset CurrentTime() => DateTimeOffset.UtcNow;
    }
}
