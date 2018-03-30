using System;

namespace Jaeger.Core
{
    [Flags]
    public enum ContextFlags
    {
        None = 0b00,
        Sampled = 0b01,
    }
}
