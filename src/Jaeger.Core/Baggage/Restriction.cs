using System.Collections.Generic;
using Jaeger.Core.Util;

namespace Jaeger.Core.Baggage
{
    /// <summary>
    /// <see cref="Restriction"/> determines whether a baggage key is allowed and contains any
    /// restrictions on the baggage value.
    /// </summary>
    public sealed class Restriction : ValueObject
    {
        // Note: In Java, this is on IBaggageRestrictionManager, but we can't put consts on interfaces in C#.
        public const int DefaultMaxValueLength = 2048;

        public bool KeyAllowed { get; }

        public int MaxValueLength { get; }

        public Restriction(bool keyAllowed, int maxValueLength)
        {
            KeyAllowed = keyAllowed;
            MaxValueLength = maxValueLength;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return KeyAllowed;
            yield return MaxValueLength;
        }
    }
}