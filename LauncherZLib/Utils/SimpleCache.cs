using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Utils
{
    /// <summary>
    /// A simple cache implementation.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Entry type.</typeparam>
    /// <remarks>
    /// This class is not thread-safe.
    /// </remarks>
    public class SimpleCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {

        private readonly int _capacity;
        private readonly Dictionary<TKey, CacheEntry> _entries;
        private readonly CacheEntry _head;
        private readonly CacheEntry _tail;

        public SimpleCache(int capacity)
        {
            _capacity = capacity;
            _entries = new Dictionary<TKey, CacheEntry>(capacity);
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
        public TValue this[TKey key]
        {
            get { return Get(key); }
            set { Put(key, value); }
        }

        /// <summary>
        /// Gets the capacity of the cache.
        /// </summary>
        public int Capacity
        {
            get { return _capacity; }
        }

        /// <summary>
        /// Gets the number of cached entries.
        /// </summary>
        public int Count
        {
            get { return _entries.Count; }
        }

        /// <summary>
        /// Checks if given entry exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return _entries.ContainsKey(key);
        }

        /// <summary>
        /// Adds a new entry to the cache.
        /// If the key already exists, the corresponding entry will be updated.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Put(TKey key, TValue value)
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

        /// <summary>
        /// Retrieves a cache entry.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public TValue Get(TKey key)
        {
            if (!_entries.ContainsKey(key))
                throw new KeyNotFoundException();
            RefreshImpl(key);
            return _entries[key].Value;
        }

        /// <summary>
        /// Safely retrieves a cache entry.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">
        /// Retrieved data.
        /// Will be the default value of TValue if given key is not found.
        /// </param>
        /// <returns>True is key exists.</returns>
        public bool TryGet(TKey key, out TValue value)
        {
            if (_entries.ContainsKey(key))
            {
                value = _entries[key].Value;
                RefreshImpl(key);
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Refreshes specified entry.
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        public void Refresh(TKey key)
        {
            if (!_entries.ContainsKey(key))
                throw new KeyNotFoundException();
            RefreshImpl(key);

        }

        /// <summary>
        /// Clear all cache entries.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
            _head.Next = _tail;
            _tail.Prev = _head;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            CacheEntry curNode = _head.Next;
            while (curNode != _tail)
            {
                yield return new KeyValuePair<TKey, TValue>(curNode.Key, curNode.Value);
                curNode = curNode.Next;
            }
        }

        /// <summary>
        /// Internal implementation of refresh function.
        /// </summary>
        /// <param name="key"></param>
        private void RefreshImpl(TKey key)
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

        /// <summary>
        /// Represents a cache entry.
        /// Also functions as a linked list node.
        /// </summary>
        class CacheEntry
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public CacheEntry Prev { get; set; }
            public CacheEntry Next { get; set; }
        }
        
    }

    

}
