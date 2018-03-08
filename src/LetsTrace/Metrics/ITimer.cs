namespace LetsTrace.Metrics
{
    public interface ITimer : IMetricValue
    {
        long MillisecondsTotal { get; }

        void DurationMicros(long time);
    }
}
