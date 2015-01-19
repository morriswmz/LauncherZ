using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Launcher
{
    /// <summary>
    /// Represents a read-only argument collection.
    /// </summary>
    public class ArgumentCollection : IReadOnlyCollection<string>
    {

        private readonly string[] _arguments;

        public ArgumentCollection(string[] arguments)
        {
            _arguments = (string[]) arguments.Clone();
            Count = _arguments.Length;
        }

        public string this[int index] { get { return _arguments[index]; } }

        public int Count { get; private set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _arguments.AsEnumerable().GetEnumerator();
        }
        
    }
}
