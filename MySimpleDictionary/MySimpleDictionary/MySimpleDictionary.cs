using MySimpleDictionary.Enums;
using MySimpleDictionary.Interfaces;
using System.Collections;

namespace MySimpleDictionary
{
    public class MySimpleDictionary<TKey, TValue> : IMySimpleDictionary<TKey, TValue>
    {
        private struct Entry {
            public TKey Key;
            public TValue Value;
            public EntryState State;
        }

        private Entry[] _entries;
        private int _count;
        private const double LoadFactor = 0.75;
        private int _deletedCount = 0;

        public int Count => _count;

        public IEnumerable<TKey> Keys 
        {
            get
            {
                for (int i = 0; i < _entries.Length; i++)
                    if (_entries[i].State == EntryState.Occupied)
                        yield return _entries[i].Key;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                for (int i = 0; i < _entries.Length; i++)
                    if (_entries[i].State == EntryState.Occupied)
                        yield return _entries[i].Value;
            }
        }

        public MySimpleDictionary(int initialCapacity = 16) {
            if (initialCapacity < 8) initialCapacity = 8;
            _entries = new Entry[initialCapacity];
            _count = 0;
        }

        private int Hash(TKey key)
        {
            if (key == null) return 0;
            return key.GetHashCode() & 0x7FFFFFFF;
        }

        private int IndexForHash(int hash, int length) => hash % length;

        private int FindIndexForKey(TKey key)
        {
            int hash = Hash(key);
            int idx = IndexForHash(hash, _entries.Length);
            int start = idx;

            while (true)
            {
                var state = _entries[idx].State;
                if (state == EntryState.Empty) return -1;
                if (state == EntryState.Occupied && EqualityComparer<TKey>.Default.Equals(_entries[idx].Key, key))
                    return idx;

                idx++;
                if (idx >= _entries.Length) idx = 0;
                if (idx == start) return -1;
            }
        }

        private int FindIndexForInsert(TKey key)
        {
            int hash = Hash(key);
            int idx = IndexForHash(hash, _entries.Length);
            int start = idx;
            int firstDeleted = -1; // nije zapamceno obrisano mjesto

            while (true)
            {
                var state = _entries[idx].State;

                if (state == EntryState.Empty)
                    return firstDeleted != -1 ? firstDeleted : idx;

                if (state == EntryState.Deleted)
                {
                    if (firstDeleted == -1) firstDeleted = idx;
                }
                else if (state == EntryState.Occupied)
                {
                    if (EqualityComparer<TKey>.Default.Equals(_entries[idx].Key, key))
                        throw new ArgumentException("Key already exists");
                }

                idx++;
                if (idx >= _entries.Length) idx = 0;
                if (idx == start)
                {
                    if (firstDeleted != -1) return firstDeleted;
                    return -1;
                }
            }
        }

        private void Resize()
        {
            int newSize = _entries.Length * 2;
            if (newSize < 8) newSize = 8;
            var old = _entries;
            _entries = new Entry[newSize];
            _count = 0;

            for (int i = 0; i < old.Length; i++)
            {
                if (old[i].State == EntryState.Occupied)
                    Add(old[i].Key, old[i].Value);
            }
        }
      
        public void Add(TKey key, TValue value)
        {
            if ((_count + 1) > _entries.Length * LoadFactor)
                Resize();

            int idx = FindIndexForInsert(key);
            if (idx == -1)
            {
                Resize();
                idx = FindIndexForInsert(key);
                if (idx == -1)
                    throw new InvalidOperationException("Insert slot not found after resize");
            }

            _entries[idx].Key = key;
            _entries[idx].Value = value;
            _entries[idx].State = EntryState.Occupied;
            _count++;
        }

        public bool ContainsKey(TKey key) => FindIndexForKey(key) != -1;

        public bool ContainsValue(TValue value)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].State == EntryState.Occupied &&
                    EqualityComparer<TValue>.Default.Equals(_entries[i].Value, value))
                    return true;
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            int idx = FindIndexForKey(key);
            if (idx == -1)
            {
                return false;
            }

            _entries[idx].State = EntryState.Deleted;
            _entries[idx].Key = default!;
            _entries[idx].Value = default!;
            _count--;
            _deletedCount++;

            // Ako ima previše izbrisanih, pokreni rehashing
            if (_deletedCount > _entries.Length / 20)
            {
                Console.WriteLine("Rehashing triggered!");
                Rehash();
            }

            return true;
        }

        private void Rehash()
        {
            var oldEntries = _entries;
            _entries = new Entry[oldEntries.Length];
            _count = 0;
            _deletedCount = 0;

            for (int i = 0; i < oldEntries.Length; i++)
            {
                if (oldEntries[i].State == EntryState.Occupied)
                {
                    Add(oldEntries[i].Key, oldEntries[i].Value);
                }
            }
        }

        public void Clear()
        {
            _entries = new Entry[_entries.Length];
            _count = 0;
        }

        public TValue this[TKey key]
        {
            get
            {
                int idx = FindIndexForKey(key);
                if (idx == -1) throw new KeyNotFoundException();
                return _entries[idx].Value;
            }
            set
            {
                int idx = FindIndexForKey(key);
                if (idx != -1)
                {
                    _entries[idx].Value = value;
                    return;
                }
                Add(key, value);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < _entries.Length; i++)
                if (_entries[i].State == EntryState.Occupied)
                    yield return new KeyValuePair<TKey, TValue>(_entries[i].Key, _entries[i].Value);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
       
    }
}
