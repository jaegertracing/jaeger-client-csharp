using System;

namespace Jaeger.Core.Exceptions
{
    public class SenderException : Exception
    {
        public int DroppedSpanCount { get; }

        public SenderException(string message, int droppedSpans) : this(message, null, droppedSpans)
        {
            DroppedSpanCount = droppedSpans;
        }

        public SenderException(string message, Exception innerException, int droppedSpans) : base(message, innerException)
        {
            DroppedSpanCount = droppedSpans;
        }
    }
}
