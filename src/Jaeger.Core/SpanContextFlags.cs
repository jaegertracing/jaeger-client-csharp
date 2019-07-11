using System;

namespace Jaeger
{
    [Flags]
    public enum SpanContextFlags
    {
        None = 0,
        Sampled = 1,
        Debug = 2,
    }
}
