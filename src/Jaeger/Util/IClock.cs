using System;

namespace Jaeger.Util
{
    public interface IClock
    {
        DateTime UtcNow();
    }
}