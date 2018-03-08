using OpenTracing.Propagation;

namespace LetsTrace.Tests
{
    internal struct Builtin<TCarrier> : IFormat<TCarrier>
    {
        private readonly string _name;

        public Builtin(string name)
        {
            _name = name;
        }

        /// <summary>Short name for built-in formats as they tend to show up in exception messages</summary>
        public override string ToString()
        {
            return $"{GetType().Name}.{_name}";
        }
    }
}
