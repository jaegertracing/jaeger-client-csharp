using System;

namespace LetsTrace.Util
{
    public interface IClock
    {
        DateTimeOffset CurrentTime();
    }
}