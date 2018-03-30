using System;

namespace Jaeger.Core.Util
{
    public interface IClock
    {
        DateTime UtcNow();
    }
}