using System;

namespace LetsTrace.Util
{
    public interface IClock
    {
        DateTime UtcNow();
    }
}