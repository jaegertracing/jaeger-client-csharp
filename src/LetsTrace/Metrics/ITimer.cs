namespace LetsTrace.Metrics
{
    // TODO: Not used now, but ported from java implementation
    public interface ITimer : IMetricValue
    {
        long MillisecondsTotal { get; }

        void DurationMicros(long time);
    }
}
