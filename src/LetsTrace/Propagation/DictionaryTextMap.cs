using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenTracing.Propagation;

namespace LetsTrace.Propagation
{
    public class DictionaryTextMap : ITextMap
    {
        private readonly IDictionary<string, string> _map;

        public DictionaryTextMap(IDictionary<string, string> map = null)
        {
            _map = map ?? new Dictionary<string, string>();
        }

        public void Set(string key, string value) => _map[key] = value;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _map.GetEnumerator();

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => _map.GetEnumerator();
    }
}
