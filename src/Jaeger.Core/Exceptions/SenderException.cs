using System;

namespace Jaeger.Core.Exceptions
{
    public class SenderException : Exception
    {
        public int DroppedSpans { get; }

        public SenderException(string message, int droppedSpans) : this(message, null, droppedSpans)
        {
            DroppedSpans = droppedSpans;
        }

        public SenderException(string message, Exception innerException, int droppedSpans) : base(message, innerException)
        {
            DroppedSpans = droppedSpans;
        }
    }
}
