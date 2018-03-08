using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace LetsTrace.Metrics
{
    internal class PropertyGrouping
    {
        public List<AttributedProperty<MetricAttribute>> Metric { get; } = new List<AttributedProperty<MetricAttribute>>();

        public PropertyGrouping(IEnumerable<PropertyInfo> properties)
        {
            foreach (var property in properties)
            {
                var metricAttribute = property.GetCustomAttribute<MetricAttribute>();
                if (metricAttribute != null)
                {
                    this.Metric.Add(new AttributedProperty<MetricAttribute>(metricAttribute, property));
                    continue;
                }

                throw new InvalidConstraintException($"Property {property.Name} does not have an attribute");
            }
        }
    }
}
