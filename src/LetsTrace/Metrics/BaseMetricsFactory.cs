namespace LetsTrace.Metrics
{
    public abstract partial class BaseMetricsFactory : IMetricsFactory
    {
        private static readonly TypeConstructor Constructor = new TypeConstructor();

        /// <summary>
        /// Creates a counter with the given gauge name and set of tags. The actual metric name is a combination of those two
        /// values. The counter starts at 0.
        /// </summary>
        /// <param name="name">The counter name</param>
        /// <param name="attribute">The attributes to add to the counter</param>
        /// <returns>A <see cref="ICounter"/> with a metric name following the counter name and tags</returns>
        protected abstract ICounter CreateCounter(string name, MetricAttribute attribute);

        /// <summary>
        /// Creates a timer with the given timer name and set of tags. The actual metric name is a combination of those two
        /// values. The timer starts at 0.
        /// </summary>
        /// <param name="name">The timer name</param>
        /// <param name="attribute">The attributes to add to the counter</param>
        /// <returns>A <see cref="ITimer"/> with a metric name following the counter name and tags</returns>
        protected abstract ITimer CreateTimer(string name, MetricAttribute attribute);

        /// <summary>
        /// Creates a gauge with the given gauge name and set of tags. The actual metric name is a combination of those two
        /// values. The timer starts at 0.
        /// </summary>
        /// <param name="name">The gauge name</param>
        /// <param name="attribute">The attributes to add to the counter</param>
        /// <returns>A <see cref="IGauge"/> with a metric name following the gauge name and tags</returns>
        protected abstract IGauge CreateGauge(string name, MetricAttribute attribute);

        public IMetrics CreateMetrics()
        {
            return Constructor.CreateMetrics(this);
        }
    }
}
