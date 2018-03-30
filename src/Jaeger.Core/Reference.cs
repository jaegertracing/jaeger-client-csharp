using OpenTracing;

namespace Jaeger.Core
{
    public struct Reference
    {
        public string Type { get; }
        public ISpanContext Context { get; }

        public Reference(string type, ISpanContext context)
        {
            Type = type;
            Context = context;
        }
    }
}