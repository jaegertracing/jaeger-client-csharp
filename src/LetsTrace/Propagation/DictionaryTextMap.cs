using System.Collections;
using System.Collections.Generic;
using OpenTracing.Propagation;

namespace LetsTrace.Propagation
{
    // TODO: make an extension method to convert dictionaries to DictionaryTextMap
    public class DictionaryTextMap : ITextMap
    {   
        private IDictionary<string, string> _map { get; }

        public DictionaryTextMap(IDictionary<string, string> map = null)
        {
            _map = map ?? new Dictionary<string, string>();
        }

        public void Set(string key, string value) => _map[key] = value;

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _map.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _map.GetEnumerator();
    }
}
