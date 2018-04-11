using System;
using System.Collections.Generic;

namespace Jaeger.Core.Metrics
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MetricAttribute : Attribute
    {
        // Note: Java allows to pass a list of tags, however C# doesn't allow complex types for attribute constructors
        // so we have to use simplified constructors.

        public string Name { get; }

        public Dictionary<string, string> Tags { get; } = new Dictionary<string, string>();

        public MetricAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public MetricAttribute(string name, string tag1Key, string tag1Value)
            : this(name)
        {
            if (tag1Key != null)
            {
                Tags[tag1Key] = tag1Value;
            }
        }

        public MetricAttribute(string name, string tag1Key, string tag1Value, string tag2Key, string tag2Value)
            : this(name, tag1Key, tag1Value)
        {
            if (tag2Key != null)
            {
                Tags[tag2Key] = tag2Value;
            }
        }
    }
}
