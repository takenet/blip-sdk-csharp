using System;
using System.Collections.Generic;

namespace Take.Blip.Client
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            return default;
        }
    }
}