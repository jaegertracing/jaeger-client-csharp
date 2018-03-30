using System;

namespace Jaeger.Core.Util
{
    public class Clock : IClock
    {
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}
