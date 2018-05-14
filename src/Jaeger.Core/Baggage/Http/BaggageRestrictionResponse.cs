using System;
using System.Collections.Generic;
using Jaeger.Core.Util;

namespace Jaeger.Core.Baggage.Http
{
    public sealed class BaggageRestrictionResponse : ValueObject
    {
        public string BaggageKey { get; }

        public int MaxValueLength { get; }

        public BaggageRestrictionResponse(string baggageKey, int maxValueLength)
        {
            BaggageKey = baggageKey ?? throw new ArgumentNullException(nameof(baggageKey));
            MaxValueLength = maxValueLength;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return BaggageKey;
            yield return MaxValueLength;
        }
    }
}