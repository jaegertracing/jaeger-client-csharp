using System;

namespace LetsTrace.Metrics
{
    [AttributeUsage(AttributeTargets.All)]
    public class MetricAttribute : Attribute
    {
        public enum MetricState
        {
            Undefined,
            Started,
            Joined
        }

        public enum MetricSampled
        {
            Undefined,
            Yes,
            No
        }

        public enum MetricResult
        {
            Undefined,
            Ok,
            Error,
            Dropped
        }

        public MetricAttribute(string name, MetricState state = MetricState.Undefined, MetricSampled sampled = MetricSampled.Undefined, MetricResult result = MetricResult.Undefined)
        {
            Name = name;
            State = state;
            Sampled = sampled;
            Result = result;
        }

        public string Name { get; }
        public MetricState State { get; }
        public MetricSampled Sampled { get; }
        public MetricResult Result { get; }
    }
}
