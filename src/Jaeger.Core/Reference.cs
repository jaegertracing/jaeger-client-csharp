using System.Collections.Generic;
using Jaeger.Core.Util;
using OpenTracing;

namespace Jaeger.Core
{
    public sealed class Reference : ValueObject
    {
        public SpanContext Context { get; }
        public string Type { get; }

        public Reference(SpanContext context, string type)
        {
            Context = context;
            Type = type;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Context;
            yield return Type;
        }
    }
}