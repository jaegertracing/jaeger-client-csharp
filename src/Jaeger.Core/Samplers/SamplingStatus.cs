using System;
using System.Collections.Generic;
using Jaeger.Core.Util;

namespace Jaeger.Core.Samplers
{
    public sealed class SamplingStatus : ValueObject
    {
        public bool IsSampled { get; }
        public IReadOnlyDictionary<string, object> Tags { get; }

        public SamplingStatus(bool isSampled, IReadOnlyDictionary<string, object> tags)
        {
            IsSampled = isSampled;
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return IsSampled;
            foreach (var tag in Tags)
            {
                yield return tag;
            }
        }
    }
}