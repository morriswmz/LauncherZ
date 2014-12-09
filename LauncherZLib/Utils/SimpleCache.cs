using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Utils
{
    /// <summary>
    /// A simple thread-safe cache implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleCache<T>
    {

        private readonly object _lock = new object();
        private readonly int _capacity;
        private readonly Dictionary<string, CacheEntry> _entries;
        private readonly CacheEntry _head;
        private readonly CacheEntry _tail;

        public SimpleCache(int capacity)
        {
            _capacity = capacity;
            _entries = new Dictionary<string, CacheEntry>(capacity);
            _head = new CacheEntry();
            _tail = new CacheEntry();
            _head.Next = _tail;
            _tail.Prev = _head;
        }

        /// <summary>
        /// Gets or sets a cache entry with given key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public T this[string key]
        {
            get { return Get(key); }
            set { Put(key, value); }
        }

        public int Capacity
        {
            get { return _capacity; }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _entries.Count;
                }
            }
        }

        public bool Contains(string key)
        {
            lock (_lock)
            {
                return _entries.ContainsKey(key);
            }
        }

        public void Put(string key, T value)
        {
            lock (_lock)
            {
                if (_entries.ContainsKey(key))
                {
                    _entries[key].Value = value;
                    RefreshImpl(key);
                }
                else
                {
                    var entry = new CacheEntry()
                    {
                        Key = key,
                        Value = value
                    };
                    // add to the beginning
                    entry.Prev = _head;
                    entry.Next = _head.Next;
                    _head.Next.Prev = entry;
                    _head.Next = entry;
                    _entries[key] = entry;
                    // check capacity
                    if (Count > Capacity)
                    {
                        CacheEntry victim = _tail.Prev;
                        _tail.Prev = victim.Prev;
                        victim.Prev.Next = _tail;
                        victim.Prev = null;
                        victim.Next = null;
                        _entries.Remove(victim.Key);
                    }
                }
            }
        }

        public T Get(string key)
        {
            lock (_lock)
            {
                if (!_entries.ContainsKey(key))
                    throw new KeyNotFoundException();
                RefreshImpl(key);
                return _entries[key].Value;
            }
        }

        public void Refresh(string key)
        {
            lock (_lock)
            {
                if (!_entries.ContainsKey(key))
                    throw new KeyNotFoundException();
                RefreshImpl(key);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _entries.Clear();
                _head.Next = _tail;
                _tail.Prev = _head;
            }
        }
        
        private void RefreshImpl(string key)
        {
            CacheEntry entry = _entries[key];
            if (entry != _head.Next)
            {
                // not the first one, do refresh
                entry.Prev.Next = entry.Next;
                entry.Next.Prev = entry.Prev;
                entry.Next = _head.Next;
                entry.Prev = _head;
                _head.Next.Prev = entry;
                _head.Next = entry;
            }
        }

        class CacheEntry
        {
            public string Key { get; set; }
            public T Value { get; set; }
            public CacheEntry Prev { get; set; }
            public CacheEntry Next { get; set; }
        }


        
    }

    

}
