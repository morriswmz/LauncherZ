using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LauncherZLib.Utils;

namespace LauncherZLib.Launcher
{
    /// <summary>
    /// Represents a collection of string-valued query parameters.
    /// </summary>
    public class QueryParameterCollection : IDictionary<string, StringValueCollection>
    {

        private readonly Dictionary<string, StringValueCollection> _storage; 

        /// <summary>
        /// Creates an empty parameter collection.
        /// </summary>
        public QueryParameterCollection()
        {
            _storage = new Dictionary<string, StringValueCollection>(0);
        }

        /// <summary>
        /// Creates a parameter collection from parameter string, which should be uri-encoded.
        /// </summary>
        /// <param name="paramStr">
        /// Parameter string extract from uri with format "key1=val1&amp;key2=val2".
        /// </param>
        /// Thrown when paramter string is not in correct format.
        /// </exception>
        public QueryParameterCollection(string paramStr)
        {
            if (paramStr == null)
                throw new ArgumentNullException("paramStr");
            if (paramStr.Any(char.IsWhiteSpace))
                throw new FormatException("Whitespace not encoded correctly.");

            string[] pairs = paramStr.Split(new char[] {'&'});
            var tempStorage = new Dictionary<string, List<string>>(); 
            foreach (var pair in pairs)
            {
                if (pair.Length == 0)
                {
                    continue;
                }
                
                var kv = pair.Split(new char[] {'='});
                if (kv.Length != 2)
                {
                    throw new FormatException(string.Format("Invalid key-value pair \"{0}\"", pair));
                }
                var key = Uri.UnescapeDataString(kv[0]);
                VerifyParameterName(key);
                var value = Uri.UnescapeDataString(kv[1]);
                if (!tempStorage.ContainsKey(key))
                {
                    tempStorage[key] = new List<string>();
                }
                tempStorage[key].Add(value);
            }
            
            _storage = new Dictionary<string, StringValueCollection>(tempStorage.Count);
            foreach (var pair in tempStorage)
            {
                _storage.Add(pair.Key, new StringValueCollection(pair.Value));
            }
            tempStorage.Clear();
        }

        /// <summary>
        /// Creates an parameter collection from existing data.
        /// </summary>
        /// <param name="data"></param>
        public QueryParameterCollection(IDictionary<string, IEnumerable<string>> data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            _storage = new Dictionary<string, StringValueCollection>();
            foreach (var pair in data)
            {
                VerifyParameterName(pair.Key);
                _storage[pair.Key] = new StringValueCollection(pair.Value);
            }
        }

        /// <summary>
        /// Creates an parameter collection based on an existing one.
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newData"></param>
        /// <exception cref="T:System.ArgumentNullException"></exception>
        /// <exception cref="T:System.FormatException"></exception>
        public QueryParameterCollection(QueryParameterCollection old, IDictionary<string, IEnumerable<string>> newData)
        {
            if (old == null)
                throw new ArgumentNullException("old");
            if (newData == null)
                throw new ArgumentNullException("newData");

            _storage = new Dictionary<string, StringValueCollection>(old);
            foreach (var pair in newData)
            {
                VerifyParameterName(pair.Key);
                _storage[pair.Key] = new StringValueCollection(pair.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, StringValueCollection>> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        void ICollection<KeyValuePair<string, StringValueCollection>>.Add(KeyValuePair<string, StringValueCollection> item)
        {
            throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection);
        }

        bool ICollection<KeyValuePair<string, StringValueCollection>>.Contains(KeyValuePair<string, StringValueCollection> item)
        {
            return ((ICollection<KeyValuePair<string, StringValueCollection>>)_storage).Contains(item);
        }

        void ICollection<KeyValuePair<string, StringValueCollection>>.CopyTo(KeyValuePair<string, StringValueCollection>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, StringValueCollection>>) _storage).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, StringValueCollection>>.Remove(KeyValuePair<string, StringValueCollection> item)
        {
            throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection);
        }

        public int Count
        {
            get { return _storage.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool ContainsKey(string key)
        {
            return _storage.ContainsKey(key);
        }

        public void Add(string key, StringValueCollection value)
        {
            throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection);
        }

        public bool Remove(string key)
        {
            throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection);
        }

        public void Clear()
        {
            throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection);
        }

        public bool TryGetValue(string key, out StringValueCollection value)
        {
            return _storage.TryGetValue(key, out value);
        }

        public StringValueCollection this[string key]
        {
            get { return _storage[key]; }
            set { throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection); }
        }

        public ICollection<string> Keys
        {
            get { return _storage.Keys; }
        }

        public ICollection<StringValueCollection> Values
        {
            get { return _storage.Values; }
        }

        /// <summary>
        /// Converts to query string with necessary escape performed.
        /// </summary>
        /// <param name="prependQuestionMark">Set to true to prepend a question mark at the beginning.</param>
        /// <returns></returns>
        public string ToQueryString(bool prependQuestionMark)
        {
            var sb = new StringBuilder(prependQuestionMark ? "?" : "");
            foreach (var pair in _storage)
            {
                if (sb.Length > 1)
                {
                    sb.Append('&');
                }
                foreach (var val in pair.Value)
                {
                    sb.Append(Uri.EscapeDataString(pair.Key));
                    sb.Append('=');
                    sb.Append(Uri.EscapeDataString(val));
                }
            }
            return sb.ToString();
        }

        private static void VerifyParameterName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Parameter name cannot be empty.");
            }
            if (name.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException(string.Format("Parameter name \"{0}\" contains white space.", name));
            }
        }

    }

    /// <summary>
    /// Represents a read-only collection of string values.
    /// </summary>
    public class StringValueCollection : IList<string>
    {
        /// <summary>
        /// Contains only a single empty string.
        /// </summary>
        public static StringValueCollection SingleEmptyString = 
            new StringValueCollection(new string[] {string.Empty});

        private readonly string[] _storage;

        public StringValueCollection(IEnumerable<string> storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            _storage = storage.ToArray();

            if (_storage.Length == 0)
                throw new ArgumentException("Storage cannot be empty.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)_storage).GetEnumerator();
        }

        public void Add(string item)
        {
            throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection);
        }

        public void Clear()
        {
            throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection);
        }

        public bool Contains(string item)
        {
            return _storage.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _storage.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection);
        }

        public int Count
        {
            get { return _storage.Length; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(string item)
        {
            for (var i = 0; i < _storage.Length; i++)
            {
                if (_storage[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, string item)
        {
            throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection);
        }

        public void RemoveAt(int index)
        {
            throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection);
        }

        public string this[int index]
        {
            get { return _storage[index]; }
            set { throw ExceptionHelper.Create(ExceptionTypes.ModifyReadonlyCollection); }
        }
    }
}
