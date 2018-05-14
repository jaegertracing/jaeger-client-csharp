using System;

namespace Jaeger.Core.Util
{
    public class SystemClock : IClock
    {
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}
