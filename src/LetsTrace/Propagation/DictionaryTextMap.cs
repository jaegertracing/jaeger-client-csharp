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
        public string Get(string key) => _map[key];

        public IEnumerable<KeyValuePair<string, string>> GetEntries() => _map;

        public void Set(string key, string value) => _map[key] = value;
    }
}
