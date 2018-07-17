using System.Collections;
using System.Collections.Generic;
using OpenTracing.Propagation;

namespace Jaeger.Tests
{
    internal class TestTextMap : ITextMap
    {
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        public void Set(string key, string value) => _values[key] = value;

        public bool ContainsKey(string key) => _values.ContainsKey(key);

        public string Get(string key) => _values.TryGetValue(key, out var value) ? value : null;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();
    }
}
