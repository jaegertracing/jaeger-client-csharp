namespace Jaeger.Core.Propagation
{
    // TODO profile and cache prefixed and unprefixed keys if necessary
    public sealed class PrefixedKeys
    {
        public string PrefixedKey(string key, string prefix)
        {
            return prefix + key;
        }

        public string UnprefixedKey(string key, string prefix)
        {
            return key.Substring(prefix.Length);
        }
    }
}