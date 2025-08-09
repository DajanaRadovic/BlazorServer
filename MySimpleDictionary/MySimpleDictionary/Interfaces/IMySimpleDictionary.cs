using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySimpleDictionary.Interfaces
{
    public interface IMySimpleDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        int Count { get; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }

        void Add(TKey key, TValue value);
        bool ContainsKey(TKey key);
        bool ContainsValue(TValue value);
        bool Remove(TKey key);
        void Clear();
        TValue this[TKey key] { get; set; }
    }
}
