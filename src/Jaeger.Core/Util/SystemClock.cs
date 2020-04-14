using System;

namespace Jaeger.Util
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}
