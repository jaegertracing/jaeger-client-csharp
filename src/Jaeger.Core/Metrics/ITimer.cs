namespace Jaeger.Core.Metrics
{
    public interface ITimer
    {
        /// <remarks>
        /// This is called "durationMicros" in Java but since everything is done via ticks in C# we use this name.
        /// </remarks>
        void DurationTicks(long ticks);
    }
}
