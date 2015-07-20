using System;
using System.Collections;
using System.Collections.Generic;

namespace LauncherZLib.Plugin.Modules
{
    /// <summary>
    /// A read only string dictionary storing parameters.
    /// </summary>
    public sealed class ParameterCollection : IDictionary<string, string>
    {
        public static readonly ParameterCollection Empty = new ParameterCollection();

        private readonly Dictionary<string, string> _parameters;

        public ParameterCollection()
        {
            _parameters = new Dictionary<string, string>(0);
        }

        public ParameterCollection(IDictionary<string, string> parameters)
        {
            _parameters = new Dictionary<string, string>(parameters);
        }

        public string this[string key]
        {
            get { return _parameters[key]; }
            set { ThrowReadOnlyAccessException(); }
        }

        public ICollection<string> Keys
        {
            get { return _parameters.Keys; }
        }

        public ICollection<string> Values
        {
            get { return _parameters.Values; }
        }

        public int Count
        {
            get { return _parameters.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            ThrowReadOnlyAccessException();
        }
        
        public bool ContainsKey(string key)
        {
            return _parameters.ContainsKey(key);
        }

        public void Add(string key, string value)
        {
            ThrowReadOnlyAccessException();
        }

        public bool Remove(string key)
        {
            ThrowReadOnlyAccessException();
            return false;
        }

        public bool TryGetValue(string key, out string value)
        {
            return _parameters.TryGetValue(key, out value);
        }
        
        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            return ((ICollection<KeyValuePair<string, string>>) _parameters).Contains(item);
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, string>>) _parameters).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            ThrowReadOnlyAccessException();
            return false;
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            ThrowReadOnlyAccessException();
        }

        private void ThrowReadOnlyAccessException()
        {
            throw new NotSupportedException("Parameter collection is read only.");
        }

    }
}
