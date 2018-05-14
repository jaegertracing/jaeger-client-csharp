using System.Collections.Generic;

namespace Jaeger.Core.Metrics
{
    /// <summary>
    /// Provides a standardized way to create metrics-related objects, like <see cref="ICounter"/>,
    /// <see cref="ITimer"/> and <see cref="IGauge"/>.
    /// </summary>
    public interface IMetricsFactory
    {
        /// <summary>
        /// Creates a counter with the given counter name and set of tags. The actual metric name is a combination of those two
        /// values. The counter starts at 0.
        /// </summary>
        /// <param name="name">The counter name.</param>
        /// <param name="tags">The tags to add to the counter.</param>
        /// <returns>A <see cref="ICounter"/> with a metric name following the counter name and tags.</returns>
        /// <seealso cref="MetricsImpl.AddTagsToMetricName"/>
        ICounter CreateCounter(string name, Dictionary<string, string> tags);

        /// <summary>
        /// Creates a timer with the given timer name and set of tags. The actual metric name is a combination of those two
        /// values. The timer starts at 0.
        /// </summary>
        /// <param name="name">The timer name.</param>
        /// <param name="tags">The tags to add to the timer.</param>
        /// <returns>A <see cref="ITimer"/> with a metric name following the timer name and tags.</returns>
        /// <seealso cref="MetricsImpl.AddTagsToMetricName"/>
        ITimer CreateTimer(string name, Dictionary<string, string> tags);

        /// <summary>
        /// Creates a gauge with the given gauge name and set of tags. The actual metric name is a combination of those two
        /// values. The gauge starts at 0.
        /// </summary>
        /// <param name="name">The gauge name.</param>
        /// <param name="tags">The tags to add to the gauge.</param>
        /// <returns>A <see cref="IGauge"/> with a metric name following the gauge name and tags.</returns>
        /// <seealso cref="MetricsImpl.AddTagsToMetricName"/>
        IGauge CreateGauge(string name, Dictionary<string, string> tags);
    }
}
